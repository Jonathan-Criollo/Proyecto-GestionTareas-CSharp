using GestionTareas.Core.Models;
using GestionTareas.Core.Services;

namespace GestionTareas.UI;

/// <summary>
/// Ventana principal de la aplicación.
///
/// Uso: Application.Run(new FormPrincipal(gestor));
///
/// Muestra las tareas en una tabla, enlaza las acciones (nueva, editar,
/// completar/reabrir, eliminar) y expone los filtros de <see cref="FiltradoDeTareas"/>.
/// No hace falta refrescar a mano: se suscribe a GestorDeTareas.TareasCambiadas.
/// </summary>
public sealed class FormPrincipal : Form
{
    // Una sola paleta, un solo acento. El resto es tinta y gris.
    private static readonly Color ColorTexto = Color.FromArgb(0x1F, 0x24, 0x2B);
    private static readonly Color ColorSecundario = Color.FromArgb(0x5B, 0x64, 0x70);
    private static readonly Color ColorAcento = Color.FromArgb(0x2F, 0x5D, 0x50);
    private static readonly Color ColorBorde = Color.FromArgb(0xC4, 0xC9, 0xD0);
    private static readonly Color ColorSeleccion = Color.FromArgb(0xE3, 0xEC, 0xE9);

    private readonly GestorDeTareas _gestor;

    private readonly TextBox _txtBuscar = new();
    private readonly ComboBox _cmbEstado = new();
    private readonly ComboBox _cmbTipo = new();
    private readonly ComboBox _cmbPrioridad = new();
    private readonly DateTimePicker _dtpDesde = new();
    private readonly DateTimePicker _dtpHasta = new();
    private readonly DataGridView _tabla = new();
    private readonly Button _btnNueva = new();
    private readonly Button _btnEditar = new();
    private readonly Button _btnCompletar = new();
    private readonly Button _btnEliminar = new();
    private readonly Label _lblResumen = new();

    public FormPrincipal(GestorDeTareas gestor)
    {
        _gestor = gestor ?? throw new ArgumentNullException(nameof(gestor));

        ConfigurarVentana();
        ConstruirControles();
        ConfigurarTabla();
        CargarOpciones();

        _gestor.TareasCambiadas += (_, _) => RefrescarTabla();
        RefrescarTabla();
    }

    private ITarea? TareaSeleccionada => _tabla.CurrentRow?.Tag as ITarea;

    // ------------------------------------------------------------------
    // Construcción de la interfaz
    // ------------------------------------------------------------------

    private void ConfigurarVentana()
    {
        Text = "Gestión de tareas";
        Font = new Font("Segoe UI", 9.5f);
        ForeColor = ColorTexto;
        BackColor = Color.White;

        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 560);
        Size = new Size(1024, 620);
        AutoScaleMode = AutoScaleMode.Dpi;
        Padding = new Padding(24);
    }

    private void ConstruirControles()
    {
        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0),
        };
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // filtros
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // tabla
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // acciones

        raiz.Controls.Add(ConstruirBarraFiltros(), 0, 0);
        raiz.Controls.Add(_tabla, 0, 1);
        raiz.Controls.Add(ConstruirBarraAcciones(), 0, 2);

        Controls.Add(raiz);
    }

    private Control ConstruirBarraFiltros()
    {
        var barra = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Margin = new Padding(0),
        };

        _txtBuscar.PlaceholderText = "Título o descripción";
        _txtBuscar.TextChanged += (_, _) => RefrescarTabla();
        barra.Controls.Add(CrearFiltro("Buscar", _txtBuscar, 210));

        ConfigurarCombo(_cmbEstado);
        barra.Controls.Add(CrearFiltro("Estado", _cmbEstado, 130));

        ConfigurarCombo(_cmbTipo);
        barra.Controls.Add(CrearFiltro("Tipo", _cmbTipo, 130));

        ConfigurarCombo(_cmbPrioridad);
        barra.Controls.Add(CrearFiltro("Prioridad", _cmbPrioridad, 125));

        ConfigurarSelectorFecha(_dtpDesde);
        barra.Controls.Add(CrearFiltro("Vence desde", _dtpDesde, 110));

        ConfigurarSelectorFecha(_dtpHasta);
        barra.Controls.Add(CrearFiltro("Vence hasta", _dtpHasta, 110));

        return barra;
    }

    private Control ConstruirBarraAcciones()
    {
        var barra = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0, 16, 0, 0),
        };
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        barra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _lblResumen.AutoSize = true;
        _lblResumen.ForeColor = ColorSecundario;
        _lblResumen.Anchor = AnchorStyles.Left; // queda centrado vertical frente a los botones
        barra.Controls.Add(_lblResumen, 0, 0);

        var botones = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
        };

        EstilizarBotonPrimario(_btnNueva, "Nueva tarea");
        _btnNueva.Click += (_, _) => NuevaTarea();

        EstilizarBotonSecundario(_btnEliminar, "Eliminar");
        _btnEliminar.Margin = new Padding(8, 0, 0, 0);
        _btnEliminar.Click += (_, _) => EliminarSeleccionada();

        EstilizarBotonSecundario(_btnCompletar, "Completar");
        _btnCompletar.Margin = new Padding(8, 0, 0, 0);
        _btnCompletar.Click += (_, _) => CambiarEstadoSeleccionada();

        EstilizarBotonSecundario(_btnEditar, "Editar");
        _btnEditar.Margin = new Padding(8, 0, 0, 0);
        _btnEditar.Click += (_, _) => EditarSeleccionada();

        // RightToLeft: el primero que se agrega queda a la derecha.
        botones.Controls.Add(_btnNueva);
        botones.Controls.Add(_btnEliminar);
        botones.Controls.Add(_btnCompletar);
        botones.Controls.Add(_btnEditar);
        barra.Controls.Add(botones, 1, 0);

        return barra;
    }

    private void ConfigurarTabla()
    {
        _tabla.Dock = DockStyle.Fill;
        _tabla.Margin = new Padding(0, 16, 0, 0);
        _tabla.ReadOnly = true;
        _tabla.AllowUserToAddRows = false;
        _tabla.AllowUserToDeleteRows = false;
        _tabla.AllowUserToResizeRows = false;
        _tabla.MultiSelect = false;
        _tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _tabla.RowHeadersVisible = false;
        _tabla.BackgroundColor = Color.White;
        _tabla.BorderStyle = BorderStyle.None;
        _tabla.GridColor = ColorBorde;
        _tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

        _tabla.EnableHeadersVisualStyles = false;
        _tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
        _tabla.ColumnHeadersDefaultCellStyle.ForeColor = ColorSecundario;
        _tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
        _tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _tabla.ColumnHeadersHeight = 30;

        _tabla.DefaultCellStyle.SelectionBackColor = ColorSeleccion;
        _tabla.DefaultCellStyle.SelectionForeColor = ColorTexto;
        _tabla.RowTemplate.Height = 28;

        _tabla.Columns.Add("colTitulo", "Título");
        _tabla.Columns.Add("colTipo", "Tipo");
        _tabla.Columns.Add("colEstado", "Estado");
        _tabla.Columns.Add("colVencimiento", "Vencimiento");
        _tabla.Columns.Add("colPrioridad", "Prioridad");
        _tabla.Columns.Add("colCreacion", "Creación");

        _tabla.Columns["colTitulo"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _tabla.Columns["colTipo"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tabla.Columns["colEstado"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tabla.Columns["colVencimiento"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tabla.Columns["colPrioridad"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tabla.Columns["colCreacion"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

        _tabla.SelectionChanged += (_, _) => ActualizarBotones();
        _tabla.CellDoubleClick += (_, _) => EditarSeleccionada();
        _tabla.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete)
            {
                EliminarSeleccionada();
            }
        };
    }

    private void CargarOpciones()
    {
        _cmbEstado.Items.Add(new Opcion<bool?>("Todas", null));
        _cmbEstado.Items.Add(new Opcion<bool?>("Pendientes", false));
        _cmbEstado.Items.Add(new Opcion<bool?>("Completadas", true));
        _cmbEstado.SelectedIndex = 0;

        _cmbTipo.Items.Add(new Opcion<TipoTarea?>("Todas", null));
        _cmbTipo.Items.Add(new Opcion<TipoTarea?>("Simple", TipoTarea.Simple));
        _cmbTipo.Items.Add(new Opcion<TipoTarea?>("Con fecha", TipoTarea.ConFecha));
        _cmbTipo.Items.Add(new Opcion<TipoTarea?>("Prioritaria", TipoTarea.Prioritaria));
        _cmbTipo.SelectedIndex = 0;

        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad?>("Todas", null));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad?>("Baja", NivelPrioridad.Baja));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad?>("Media", NivelPrioridad.Media));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad?>("Alta", NivelPrioridad.Alta));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad?>("Crítica", NivelPrioridad.Critica));
        _cmbPrioridad.SelectedIndex = 0;
    }

    // ------------------------------------------------------------------
    // Filtros y tabla
    // ------------------------------------------------------------------

    /// <summary>
    /// Encadena los filtros del componente FiltradoDeTareas según lo elegido en la barra.
    /// </summary>
    private IEnumerable<ITarea> AplicarFiltros()
    {
        IEnumerable<ITarea> tareas = _gestor.ObtenerTodas();

        tareas = tareas.ContieneTexto(_txtBuscar.Text);

        if (_cmbEstado.SelectedItem is Opcion<bool?> estado && estado.Valor.HasValue)
        {
            tareas = tareas.PorEstado(estado.Valor.Value);
        }

        if (_cmbTipo.SelectedItem is Opcion<TipoTarea?> tipo && tipo.Valor.HasValue)
        {
            tareas = tareas.PorTipo(tipo.Valor.Value);
        }

        if (_cmbPrioridad.SelectedItem is Opcion<NivelPrioridad?> prioridad && prioridad.Valor.HasValue)
        {
            tareas = tareas.PorPrioridad(prioridad.Valor.Value);
        }

        // El rango solo se aplica cuando ambas casillas están marcadas.
        if (_dtpDesde.Checked && _dtpHasta.Checked)
        {
            tareas = tareas.PorRangoDeFechas(_dtpDesde.Value, _dtpHasta.Value);
        }

        return tareas;
    }

    private void RefrescarTabla()
    {
        var idSeleccionado = TareaSeleccionada?.Id;
        var tareas = AplicarFiltros().ToList();

        _tabla.SuspendLayout();
        _tabla.Rows.Clear();

        foreach (var tarea in tareas)
        {
            int fila = _tabla.Rows.Add(
                tarea.Titulo,
                TextoTipo(tarea.Tipo),
                tarea.Completada ? "Completada" : "Pendiente",
                tarea.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "—",
                tarea is TareaPrioritaria prioritaria ? TextoPrioridad(prioritaria.Prioridad) : "—",
                tarea.FechaCreacion.ToString("dd/MM/yyyy"));

            var renglon = _tabla.Rows[fila];
            renglon.Tag = tarea;

            if (tarea.Completada)
            {
                renglon.DefaultCellStyle.ForeColor = ColorSecundario;
            }
        }

        _tabla.ResumeLayout();

        // Se conserva la selección entre refrescos (p. ej., al volver del modal de edición).
        if (idSeleccionado is Guid id)
        {
            foreach (DataGridViewRow renglon in _tabla.Rows)
            {
                if (renglon.Tag is ITarea actual && actual.Id == id)
                {
                    renglon.Selected = true;
                    break;
                }
            }
        }

        _lblResumen.Text = tareas.Count == _gestor.Total
            ? $"{_gestor.Total} tareas"
            : $"{tareas.Count} de {_gestor.Total} tareas";

        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        var tarea = TareaSeleccionada;
        bool haySeleccion = tarea is not null;

        _btnEditar.Enabled = haySeleccion;
        _btnEliminar.Enabled = haySeleccion;
        _btnCompletar.Enabled = haySeleccion;
        _btnCompletar.Text = tarea?.Completada == true ? "Reabrir" : "Completar";
    }

    // ------------------------------------------------------------------
    // Acciones
    // ------------------------------------------------------------------

    private void NuevaTarea()
    {
        using var modal = new FormTareaModal(_gestor);
        modal.ShowDialog(this);
        // Si se guardó, TareasCambiadas refresca la tabla.
    }

    private void EditarSeleccionada()
    {
        if (TareaSeleccionada is not ITarea tarea)
        {
            return;
        }

        using var modal = new FormTareaModal(_gestor, tarea);
        modal.ShowDialog(this);
    }

    private void CambiarEstadoSeleccionada()
    {
        if (TareaSeleccionada is not ITarea tarea)
        {
            return;
        }

        EjecutarSeguro(() => _gestor.CambiarEstado(tarea.Id, !tarea.Completada));
    }

    private void EliminarSeleccionada()
    {
        if (TareaSeleccionada is not ITarea tarea)
        {
            return;
        }

        var respuesta = MessageBox.Show(
            this,
            $"¿Eliminar la tarea \"{tarea.Titulo}\"? Esta acción no se puede deshacer.",
            "Eliminar tarea",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (respuesta == DialogResult.Yes)
        {
            EjecutarSeguro(() => _gestor.Eliminar(tarea.Id));
        }
    }

    private void EjecutarSeguro(Action accion)
    {
        try
        {
            accion();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
                                       or KeyNotFoundException or IOException)
        {
            // Respaldo: si el Core o el archivo falla, la ventana sigue viva.
            MessageBox.Show(
                this,
                $"No se pudo completar la acción.\n\n{ex.Message}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    // ------------------------------------------------------------------
    // Utilidades de interfaz
    // ------------------------------------------------------------------

    /// <summary>Etiqueta arriba del campo, como pide DESIGN.md.</summary>
    private static TableLayoutPanel CrearFiltro(string texto, Control control, int ancho)
    {
        var pila = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Margin = new Padding(0, 0, 12, 0),
        };
        pila.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ancho));

        var etiqueta = new Label
        {
            Text = texto,
            AutoSize = true,
            ForeColor = ColorSecundario,
            Margin = new Padding(0, 0, 0, 4),
        };

        control.Width = ancho;
        control.Margin = new Padding(0);

        pila.Controls.Add(etiqueta, 0, 0);
        pila.Controls.Add(control, 0, 1);
        return pila;
    }

    private void ConfigurarCombo(ComboBox combo)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.SelectedIndexChanged += (_, _) => RefrescarTabla();
    }

    private void ConfigurarSelectorFecha(DateTimePicker selector)
    {
        selector.Format = DateTimePickerFormat.Short;
        selector.ShowCheckBox = true; // la casilla apagada significa "sin filtro de fecha"
        selector.Checked = false;
        selector.ValueChanged += (_, _) => RefrescarTabla();
    }

    private static string TextoTipo(TipoTarea tipo) => tipo switch
    {
        TipoTarea.Simple => "Simple",
        TipoTarea.ConFecha => "Con fecha",
        _ => "Prioritaria",
    };

    private static string TextoPrioridad(NivelPrioridad prioridad) => prioridad switch
    {
        NivelPrioridad.Baja => "Baja",
        NivelPrioridad.Media => "Media",
        NivelPrioridad.Alta => "Alta",
        _ => "Crítica",
    };

    private void EstilizarBotonPrimario(Button boton, string texto)
    {
        boton.Text = texto;
        boton.AutoSize = true;
        boton.MinimumSize = new Size(130, 34);
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.BackColor = ColorAcento;
        boton.ForeColor = Color.White;
        boton.Cursor = Cursors.Hand;
        boton.Margin = new Padding(8, 0, 0, 0);
        boton.UseVisualStyleBackColor = false;
    }

    private void EstilizarBotonSecundario(Button boton, string texto)
    {
        boton.Text = texto;
        boton.AutoSize = true;
        boton.MinimumSize = new Size(90, 34);
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderColor = ColorBorde;
        boton.FlatAppearance.BorderSize = 1;
        boton.BackColor = Color.White;
        boton.ForeColor = ColorTexto;
        boton.Cursor = Cursors.Hand;
        boton.Margin = new Padding(0);
        boton.UseVisualStyleBackColor = false;
    }

    /// <summary>Par texto/valor para mostrar opciones en español dentro de un ComboBox.</summary>
    private sealed class Opcion<T>
    {
        public string Texto { get; }
        public T Valor { get; }

        public Opcion(string texto, T valor)
        {
            Texto = texto;
            Valor = valor;
        }

        public override string ToString() => Texto;
    }
}
