using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace parcial_2
{
    public partial class frmProductos : Form
    {
        // Cadena de conexion a SQL Server (si tu instancia se llama distinto, cambia solo Server=)
        private const string connectionString =
            @"Server=.\SQLEXPRESS;Database=ParcialProgramacion1;Trusted_Connection=True;TrustServerCertificate=True;";

        // Id del producto seleccionado en el DataGridView (0 = ninguno)
        private int idSeleccionado = 0;

        public frmProductos()
        {
            InitializeComponent();
        }

        // ---------------------------------------------------------------
        // CARGA DEL FORMULARIO
        // ---------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            LimpiarCampos();
            CargarProductos();
        }

        // ---------------------------------------------------------------
        // Metodos vacios creados por doble clic sin querer en el diseno.
        // No hacen nada, pero el Designer los referencia, asi que se dejan.
        // ---------------------------------------------------------------
        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void txtCategoria_TextChanged(object sender, EventArgs e)
        {
        }

        // ---------------------------------------------------------------
        // GUARDAR: registra un producto nuevo (INSERT parametrizado)
        // ---------------------------------------------------------------
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos(out string nombre, out string categoria,
                               out decimal precio, out int existencias))
                return;

            const string sql =
                "INSERT INTO dbo.Productos (Nombre, Categoria, Precio, Existencias) " +
                "VALUES (@Nombre, @Categoria, @Precio, @Existencias)";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Categoria", categoria);
                    cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = precio;
                    cmd.Parameters.AddWithValue("@Existencias", existencias);

                    con.Open();
                    int filas = cmd.ExecuteNonQuery();

                    if (filas > 0)
                    {
                        MessageBox.Show("Producto registrado correctamente.", "Guardar",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LimpiarCampos();
                        CargarProductos();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        // CONSULTAR: muestra todos los productos en el DataGridView
        // ---------------------------------------------------------------
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            CargarProductos();
        }

        private void CargarProductos()
        {
            const string sql =
                "SELECT IdProducto, Nombre, Categoria, Precio, Existencias " +
                "FROM dbo.Productos ORDER BY IdProducto";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                {
                    DataTable tabla = new DataTable();
                    da.Fill(tabla);          // el adapter abre y cierra la conexion
                    dgvProductos.DataSource = tabla;
                    dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "N2";
                    dgvProductos.ClearSelection();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos al consultar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        // SELECCIONAR una fila del DataGridView: pasa los datos a los campos
        // ---------------------------------------------------------------
        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];
            idSeleccionado = Convert.ToInt32(fila.Cells["IdProducto"].Value);
            txtNombre.Text = Convert.ToString(fila.Cells["Nombre"].Value);
            txtCategoria.Text = Convert.ToString(fila.Cells["Categoria"].Value);
            txtPrecio.Text = Convert.ToDecimal(fila.Cells["Precio"].Value)
                                .ToString("0.00", CultureInfo.CurrentCulture);
            txtExistencias.Text = Convert.ToString(fila.Cells["Existencias"].Value);

            EstablecerModo(seleccionado: true);
        }

        // ---------------------------------------------------------------
        // ACTUALIZAR: modifica el producto seleccionado (UPDATE parametrizado)
        // ---------------------------------------------------------------
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            int id = idSeleccionado;
            if (id <= 0)
            {
                MessageBox.Show("Seleccione un producto de la tabla para actualizarlo.",
                    "Actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidarCampos(out string nombre, out string categoria,
                               out decimal precio, out int existencias))
                return;

            const string sql =
                "UPDATE dbo.Productos SET Nombre = @Nombre, Categoria = @Categoria, " +
                "Precio = @Precio, Existencias = @Existencias WHERE IdProducto = @Id";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Categoria", categoria);
                    cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = precio;
                    cmd.Parameters.AddWithValue("@Existencias", existencias);
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    int filas = cmd.ExecuteNonQuery();

                    if (filas > 0)
                    {
                        MessageBox.Show("Producto actualizado correctamente.", "Actualizar",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LimpiarCampos();
                        CargarProductos();
                    }
                    else
                    {
                        MessageBox.Show("No se encontro el producto (puede haber sido eliminado).",
                            "Actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos al actualizar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        // ELIMINAR: borra el producto seleccionado, pidiendo confirmacion
        // ---------------------------------------------------------------
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int id = idSeleccionado;
            if (id <= 0)
            {
                MessageBox.Show("Seleccione un producto de la tabla para eliminarlo.",
                    "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "Esta seguro de eliminar el producto \"" + txtNombre.Text + "\"?",
                "Confirmar eliminacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes) return;

            const string sql = "DELETE FROM dbo.Productos WHERE IdProducto = @Id";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    int filas = cmd.ExecuteNonQuery();

                    if (filas > 0)
                    {
                        MessageBox.Show("Producto eliminado correctamente.", "Eliminar",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LimpiarCampos();
                        CargarProductos();
                    }
                    else
                    {
                        MessageBox.Show("No se encontro el producto.", "Eliminar",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos al eliminar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        // LIMPIAR: vacia los campos del formulario
        // ---------------------------------------------------------------
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            idSeleccionado = 0;
            txtNombre.Clear();
            txtCategoria.Clear();
            txtPrecio.Clear();
            txtExistencias.Clear();
            dgvProductos.ClearSelection();
            EstablecerModo(seleccionado: false);
            txtNombre.Focus();
        }

        // Habilita Actualizar/Eliminar solo cuando hay un producto seleccionado
        private void EstablecerModo(bool seleccionado)
        {
            btnGuardar.Enabled = !seleccionado;
            btnActualizar.Enabled = seleccionado;
            btnEliminar.Enabled = seleccionado;
        }

        // ---------------------------------------------------------------
        // VALIDACIONES
        // ---------------------------------------------------------------
        private bool ValidarCampos(out string nombre, out string categoria,
                                   out decimal precio, out int existencias)
        {
            nombre = txtNombre.Text.Trim();
            categoria = txtCategoria.Text.Trim();
            precio = 0;
            existencias = 0;

            if (nombre.Length == 0)
            {
                Advertir("El nombre es obligatorio.", txtNombre);
                return false;
            }
            if (nombre.Length > 100)
            {
                Advertir("El nombre no puede pasar de 100 caracteres.", txtNombre);
                return false;
            }
            if (categoria.Length == 0)
            {
                Advertir("La categoria es obligatoria.", txtCategoria);
                return false;
            }
            if (categoria.Length > 50)
            {
                Advertir("La categoria no puede pasar de 50 caracteres.", txtCategoria);
                return false;
            }

            // Acepta "12.50" o "12,50" segun la configuracion regional
            string textoPrecio = txtPrecio.Text.Trim();
            if (!decimal.TryParse(textoPrecio, NumberStyles.Number, CultureInfo.CurrentCulture, out precio) &&
                !decimal.TryParse(textoPrecio, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
            {
                Advertir("El precio debe ser un numero valido.", txtPrecio);
                return false;
            }
            if (precio <= 0)
            {
                Advertir("El precio debe ser mayor que cero.", txtPrecio);
                return false;
            }
            if (precio > 99999999.99m)
            {
                Advertir("El precio es demasiado grande.", txtPrecio);
                return false;
            }

            if (!int.TryParse(txtExistencias.Text.Trim(), NumberStyles.Integer,
                              CultureInfo.InvariantCulture, out existencias))
            {
                Advertir("Las existencias deben ser un numero entero.", txtExistencias);
                return false;
            }
            if (existencias < 0)
            {
                Advertir("Las existencias deben ser igual o mayor que cero.", txtExistencias);
                return false;
            }

            return true;
        }

        private void Advertir(string mensaje, Control campo)
        {
            MessageBox.Show(mensaje, "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            campo.Focus();
        }

        private void fi11(object sender, DataGridViewAutoSizeColumnsModeEventArgs e)
        {

        }
    }
}