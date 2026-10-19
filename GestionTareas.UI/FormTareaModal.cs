using GestionTareas.Core.Models;
using GestionTareas.Core.Services;

namespace GestionTareas.UI;

/// <summary>
/// Ventana modal para crear o editar una tarea.
///
/// Uso (crear):   using var f = new FormTareaModal(gestor);              f.ShowDialog(this);
/// Uso (editar):  using var f = new FormTareaModal(gestor, tareaActual);  f.ShowDialog(this);
///
/// Devuelve DialogResult.OK solo cuando la tarea se guardó. El formulario principal
/// no necesita refrescar a mano: GestorDeTareas dispara TareasCambiadas.
/// </summary>
public sealed class FormTareaModal : Form
{
    private const int MaxTitulo = 100;
    private const int MaxDescripcion = 500;
    private const int AnchoContenido = 400;

    // Una sola paleta, un solo acento. El resto es tinta y gris.
    private static readonly Color ColorTexto = Color.FromArgb(0x1F, 0x24, 0x2B);
    private static readonly Color ColorSecundario = Color.FromArgb(0x5B, 0x64, 0x70);
    private static readonly Color ColorAcento = Color.FromArgb(0x2F, 0x5D, 0x50);
    private static readonly Color ColorError = Color.FromArgb(0xB3, 0x26, 0x1E);
    private static readonly Color ColorBorde = Color.FromArgb(0xC4, 0xC9, 0xD0);

    private readonly GestorDeTareas _gestor;
    private readonly ITarea? _tareaEditada;

    private readonly ComboBox _cmbTipo = new();
    private readonly Label _lblAyudaTipo = new();
    private readonly TextBox _txtTitulo = new();
    private readonly Label _errTitulo = new();
    private readonly TextBox _txtDescripcion = new();
    private readonly Label _lblFecha = new();
    private readonly DateTimePicker _dtpFecha = new();
    private readonly Label _errFecha = new();
    private readonly Label _lblPrioridad = new();
    private readonly ComboBox _cmbPrioridad = new();
    private readonly Button _btnGuardar = new();
    private readonly Button _btnCancelar = new();

    private bool _fechaOpcionalMarcada;

    /// <summary>La tarea creada o editada. Es null si el usuario canceló.</summary>
    public ITarea? TareaGuardada { get; private set; }

    private bool EsEdicion => _tareaEditada is not null;

    /// <param name="gestor">Gestor que persiste los cambios.</param>
    /// <param name="tareaAEditar">null para crear una tarea nueva.</param>
    public FormTareaModal(GestorDeTareas gestor, ITarea? tareaAEditar = null)
    {
        _gestor = gestor ?? throw new ArgumentNullException(nameof(gestor));
        _tareaEditada = tareaAEditar;

        ConfigurarVentana();
        ConstruirControles();
        CargarOpciones();

        if (EsEdicion)
        {
            CargarTareaExistente(_tareaEditada!);
        }
        else
        {
            _cmbTipo.SelectedIndex = 0;
            _dtpFecha.Value = DateTime.Today.AddDays(1);
            _dtpFecha.Checked = false;
        }

        AplicarTipo();
    }

    // ------------------------------------------------------------------
    // Construcción de la interfaz
    // ------------------------------------------------------------------

    private void ConfigurarVentana()
    {
        Text = EsEdicion ? "Editar tarea" : "Nueva tarea";
        Font = new Font("Segoe UI", 9.5f);
        ForeColor = ColorTexto;
        BackColor = Color.White;

        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;

        AutoScaleMode = AutoScaleMode.Dpi;
        Padding = new Padding(24);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
    }

    private void ConstruirControles()
    {
        var raiz = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Fill, // respeta el Padding del formulario (24 px de margen)
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoContenido));

        // Tipo
        AgregarFila(raiz, CrearEtiqueta("Tipo de tarea", primera: true));
        _cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbTipo.Enabled = !EsEdicion; // el Core no permite cambiar el tipo de una tarea existente
        _cmbTipo.SelectedIndexChanged += (_, _) => AplicarTipo();
        AgregarFila(raiz, _cmbTipo);

        _lblAyudaTipo.AutoSize = true;
        _lblAyudaTipo.MaximumSize = new Size(AnchoContenido, 0);
        _lblAyudaTipo.ForeColor = ColorSecundario;
        _lblAyudaTipo.Margin = new Padding(0, 4, 0, 0);
        AgregarFila(raiz, _lblAyudaTipo);

        // Título
        AgregarFila(raiz, CrearEtiqueta("Título"));
        _txtTitulo.MaxLength = MaxTitulo;
        _txtTitulo.TextChanged += (_, _) => OcultarError(_errTitulo);
        AgregarFila(raiz, _txtTitulo);
        ConfigurarEtiquetaError(_errTitulo);
        AgregarFila(raiz, _errTitulo);

        // Descripción
        AgregarFila(raiz, CrearEtiqueta("Descripción (opcional)"));
        _txtDescripcion.Multiline = true;
        _txtDescripcion.AcceptsReturn = true;
        _txtDescripcion.ScrollBars = ScrollBars.Vertical;
        _txtDescripcion.MaxLength = MaxDescripcion;
        _txtDescripcion.Height = 84;
        AgregarFila(raiz, _txtDescripcion);

        // Fecha de vencimiento (solo ConFecha y Prioritaria)
        ConfigurarEtiqueta(_lblFecha, "Fecha de vencimiento");
        AgregarFila(raiz, _lblFecha);
        _dtpFecha.Format = DateTimePickerFormat.Short;
        _dtpFecha.Width = 170;
        _dtpFecha.Anchor = AnchorStyles.Left;
        _dtpFecha.ShowCheckBox = true; // arranca con casilla para que Checked sea editable; AplicarTipo la ajusta
        _dtpFecha.ValueChanged += (_, _) =>
        {
            OcultarError(_errFecha);
            if (_dtpFecha.ShowCheckBox)
            {
                _fechaOpcionalMarcada = _dtpFecha.Checked;
            }
        };
        AgregarFila(raiz, _dtpFecha);
        ConfigurarEtiquetaError(_errFecha);
        AgregarFila(raiz, _errFecha);

        // Prioridad (solo Prioritaria)
        ConfigurarEtiqueta(_lblPrioridad, "Prioridad");
        AgregarFila(raiz, _lblPrioridad);
        _cmbPrioridad.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbPrioridad.Width = 170;
        _cmbPrioridad.Anchor = AnchorStyles.Left;
        AgregarFila(raiz, _cmbPrioridad);

        // Botones
        var barraBotones = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 24, 0, 0),
            Anchor = AnchorStyles.Right,
        };

        EstilizarBotonPrimario(_btnGuardar, EsEdicion ? "Guardar cambios" : "Crear tarea");
        _btnGuardar.Click += (_, _) => Guardar();

        EstilizarBotonSecundario(_btnCancelar, "Cancelar");
        _btnCancelar.DialogResult = DialogResult.Cancel;

        // RightToLeft: el primero que se agrega queda a la derecha.
        barraBotones.Controls.Add(_btnGuardar);
        barraBotones.Controls.Add(_btnCancelar);
        AgregarFila(raiz, barraBotones);

        AcceptButton = _btnGuardar;
        CancelButton = _btnCancelar;

        Controls.Add(raiz);
    }

    private void CargarOpciones()
    {
        _cmbTipo.Items.Add(new Opcion<TipoTarea>("Simple", TipoTarea.Simple));
        _cmbTipo.Items.Add(new Opcion<TipoTarea>("Con fecha", TipoTarea.ConFecha));
        _cmbTipo.Items.Add(new Opcion<TipoTarea>("Prioritaria", TipoTarea.Prioritaria));

        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad>("Baja", NivelPrioridad.Baja));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad>("Media", NivelPrioridad.Media));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad>("Alta", NivelPrioridad.Alta));
        _cmbPrioridad.Items.Add(new Opcion<NivelPrioridad>("Crítica", NivelPrioridad.Critica));
        _cmbPrioridad.SelectedIndex = 1; // Media, igual que el valor por defecto de la fábrica
    }

    private void CargarTareaExistente(ITarea tarea)
    {
        _cmbTipo.SelectedIndex = tarea.Tipo switch
        {
            TipoTarea.Simple => 0,
            TipoTarea.ConFecha => 1,
            _ => 2,
        };

        _txtTitulo.Text = tarea.Titulo;
        _txtDescripcion.Text = tarea.Descripcion;

        if (tarea.FechaVencimiento is DateTime fecha)
        {
            _dtpFecha.Value = fecha;
            _dtpFecha.Checked = true;
        }
        else
        {
            _dtpFecha.Value = DateTime.Today.AddDays(1);
            _dtpFecha.Checked = false;
        }

        if (tarea is TareaPrioritaria prioritaria)
        {
            _cmbPrioridad.SelectedIndex = (int)prioritaria.Prioridad;
        }
    }

    // ------------------------------------------------------------------
    // Comportamiento según el tipo de tarea
    // ------------------------------------------------------------------

    private TipoTarea TipoSeleccionado =>
        ((Opcion<TipoTarea>)_cmbTipo.SelectedItem!).Valor;

    private void AplicarTipo()
    {
        var tipo = TipoSeleccionado;

        bool usaFecha = tipo != TipoTarea.Simple;
        _lblFecha.Visible = usaFecha;
        _dtpFecha.Visible = usaFecha;
        _errFecha.Visible = usaFecha && !string.IsNullOrEmpty(_errFecha.Text);

        // En Prioritaria la fecha es opcional (casilla); en ConFecha es obligatoria.
        bool fechaOpcional = tipo == TipoTarea.Prioritaria;
        if (fechaOpcional != _dtpFecha.ShowCheckBox)
        {
            bool marcadaAntes = _fechaOpcionalMarcada;
            _dtpFecha.ShowCheckBox = fechaOpcional;
            if (fechaOpcional)
            {
                _dtpFecha.Checked = marcadaAntes; // recuerda la elección si el usuario cambia de tipo
            }
        }

        _lblFecha.Text = tipo == TipoTarea.Prioritaria
            ? "Fecha de vencimiento (opcional)"
            : "Fecha de vencimiento";

        bool usaPrioridad = tipo == TipoTarea.Prioritaria;
        _lblPrioridad.Visible = usaPrioridad;
        _cmbPrioridad.Visible = usaPrioridad;

        _lblAyudaTipo.Text = tipo switch
        {
            TipoTarea.Simple => "Un recordatorio sin fecha límite.",
            TipoTarea.ConFecha => "Necesita una fecha de vencimiento.",
            _ => "Lleva un nivel de prioridad. La fecha es opcional.",
        };
    }

    // ------------------------------------------------------------------
    // Validación y guardado
    // ------------------------------------------------------------------

    private DateTime? FechaSeleccionada()
    {
        return TipoSeleccionado switch
        {
            TipoTarea.Simple => null,
            TipoTarea.ConFecha => _dtpFecha.Value.Date,
            _ => _dtpFecha.Checked ? _dtpFecha.Value.Date : null,
        };
    }

    private bool Validar()
    {
        Control? primerError = null;

        if (string.IsNullOrWhiteSpace(_txtTitulo.Text))
        {
            MostrarError(_errTitulo, "Escribe un título para la tarea.");
            primerError ??= _txtTitulo;
        }
        else
        {
            OcultarError(_errTitulo);
        }

        var fecha = FechaSeleccionada();
        if (fecha is DateTime f && f < DateTime.Today && FechaCambioONueva(f))
        {
            MostrarError(_errFecha, "La fecha no puede ser anterior a hoy.");
            primerError ??= _dtpFecha;
        }
        else
        {
            OcultarError(_errFecha);
        }

        primerError?.Focus();
        return primerError is null;
    }

    /// <summary>
    /// Al editar, una tarea ya vencida puede conservar su fecha original sin que
    /// el formulario se queje; solo se exige "no anterior a hoy" si la fecha es nueva o cambió.
    /// </summary>
    private bool FechaCambioONueva(DateTime fecha)
    {
        return _tareaEditada?.FechaVencimiento?.Date != fecha.Date;
    }

    private void Guardar()
    {
        if (!Validar())
        {
            return;
        }

        var tipo = TipoSeleccionado;
        var titulo = _txtTitulo.Text.Trim();
        var descripcion = _txtDescripcion.Text.Trim();
        var fecha = FechaSeleccionada();
        var prioridad = ((Opcion<NivelPrioridad>)_cmbPrioridad.SelectedItem!).Valor;

        try
        {
            if (_tareaEditada is null)
            {
                // Crear: la tarea nace en TareaFactory a través del gestor.
                TareaGuardada = _gestor.Agregar(tipo, titulo, descripcion, fecha, prioridad);
            }
            else
            {
                _gestor.Editar(
                    _tareaEditada.Id,
                    titulo,
                    descripcion,
                    fecha,
                    tipo == TipoTarea.Prioritaria ? prioridad : null);
                TareaGuardada = _tareaEditada;
            }

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
                                       or KeyNotFoundException or IOException)
        {
            // Respaldo: el Core también valida. Si algo se escapa, no se pierde lo escrito.
            MessageBox.Show(
                this,
                $"No se pudo guardar la tarea.\n\n{ex.Message}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    // ------------------------------------------------------------------
    // Utilidades de interfaz
    // ------------------------------------------------------------------

    private static void AgregarFila(TableLayoutPanel tabla, Control control)
    {
        int fila = tabla.RowCount;
        tabla.RowCount = fila + 1;
        tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        if (control.Anchor == (AnchorStyles.Top | AnchorStyles.Left))
        {
            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        }

        tabla.Controls.Add(control, 0, fila);
    }

    private Label CrearEtiqueta(string texto, bool primera = false)
    {
        var etiqueta = new Label();
        ConfigurarEtiqueta(etiqueta, texto);
        if (primera)
        {
            etiqueta.Margin = new Padding(0, 0, 0, 4);
        }
        return etiqueta;
    }

    private void ConfigurarEtiqueta(Label etiqueta, string texto)
    {
        etiqueta.Text = texto;
        etiqueta.AutoSize = true;
        etiqueta.ForeColor = ColorSecundario;
        etiqueta.Margin = new Padding(0, 14, 0, 4);
    }

    private void ConfigurarEtiquetaError(Label etiqueta)
    {
        etiqueta.AutoSize = true;
        etiqueta.MaximumSize = new Size(AnchoContenido, 0);
        etiqueta.ForeColor = ColorError;
        etiqueta.Margin = new Padding(0, 4, 0, 0);
        etiqueta.Visible = false;
    }

    private static void MostrarError(Label etiqueta, string mensaje)
    {
        etiqueta.Text = mensaje;
        etiqueta.Visible = true;
    }

    private static void OcultarError(Label etiqueta)
    {
        etiqueta.Text = string.Empty;
        etiqueta.Visible = false;
    }

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

    /// <summary>Par texto/valor para mostrar enums en español dentro de un ComboBox.</summary>
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
