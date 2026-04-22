using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArbolExpresiones
{
    // ─────────────────────────────────────────
    //  NODO DEL ÁRBOL
    // ─────────────────────────────────────────
    public class NodoArbol
    {
        public string Valor;
        public NodoArbol Izquierdo;
        public NodoArbol Derecho;
        public int X, Y;

        public NodoArbol(string valor)
        {
            Valor = valor;
        }

        // Devuelve cuántos niveles tiene el árbol
        public int Profundidad()
        {
            int izq = Izquierdo?.Profundidad() ?? 0;
            int der = Derecho?.Profundidad() ?? 0;
            return 1 + Math.Max(izq, der);
        }
    }

    // ─────────────────────────────────────────
    //  PARSER (convierte texto → árbol)
    // ─────────────────────────────────────────
    public class Parser
    {
        private string _texto;
        private int _pos;

        public Parser(string texto)
        {
            _texto = texto.Replace(" ", "");
            _pos = 0;
        }

        public NodoArbol Parse()
        {
            var resultado = ParseSuma();
            if (_pos != _texto.Length)
                throw new Exception($"Carácter inesperado en posición {_pos}: '{_texto[_pos]}'");
            return resultado;
        }

        // Nivel 1: + y -  (menor precedencia)
        private NodoArbol ParseSuma()
        {
            var izq = ParseMulti();
            while (_pos < _texto.Length && (_texto[_pos] == '+' || _texto[_pos] == '-'))
            {
                char op = _texto[_pos++];
                var der = ParseMulti();
                var nodo = new NodoArbol(op.ToString());
                nodo.Izquierdo = izq;
                nodo.Derecho = der;
                izq = nodo;
            }
            return izq;
        }

        // Nivel 2: * y /  (mayor precedencia)
        private NodoArbol ParseMulti()
        {
            var izq = ParsePrimario();
            while (_pos < _texto.Length && (_texto[_pos] == '*' || _texto[_pos] == '/'))
            {
                char op = _texto[_pos++];
                var der = ParsePrimario();
                var nodo = new NodoArbol(op.ToString());
                nodo.Izquierdo = izq;
                nodo.Derecho = der;
                izq = nodo;
            }
            return izq;
        }

        // Nivel 3: números y paréntesis
        private NodoArbol ParsePrimario()
        {
            if (_pos >= _texto.Length)
                throw new Exception("Expresión incompleta.");

            if (_texto[_pos] == '(')
            {
                _pos++;
                var nodo = ParseSuma();
                if (_pos >= _texto.Length || _texto[_pos] != ')')
                    throw new Exception("Falta el paréntesis de cierre ')'.");
                _pos++;
                return nodo;
            }

            int inicio = _pos;
            if (_texto[_pos] == '-') _pos++;
            while (_pos < _texto.Length && (char.IsDigit(_texto[_pos]) || _texto[_pos] == '.'))
                _pos++;

            if (_pos == inicio || (_pos == inicio + 1 && _texto[inicio] == '-'))
                throw new Exception($"Se esperaba un número en posición {_pos}.");

            return new NodoArbol(_texto.Substring(inicio, _pos - inicio));
        }
    }

    // ─────────────────────────────────────────
    //  FORMULARIO PRINCIPAL
    // ─────────────────────────────────────────
    public partial class Form1 : Form
    {
        private TextBox txtExpresion;
        private Button btnGenerar;
        private Panel pnlArbol;
        private Label lblTitulo;
        private Label lblInstruccion;
        private NodoArbol raiz = null;

        public Form1()
        {
            InitializeComponent();
            ConfigurarUI();
        }

        private void ConfigurarUI()
        {
            this.Text = "Árbol de Expresiones";
            this.Size = new Size(900, 680);
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);

            lblTitulo = new Label();
            lblTitulo.Text = "Árbol de Expresiones Aritméticas";
            lblTitulo.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(33, 37, 41);
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(20, 15);

            lblInstruccion = new Label();
            lblInstruccion.Text = "Operadores soportados:  +  -  *  /     Ejemplo: 3 * (9 - 3 * 4)";
            lblInstruccion.Font = new Font("Segoe UI", 9);
            lblInstruccion.ForeColor = Color.FromArgb(108, 117, 125);
            lblInstruccion.AutoSize = true;
            lblInstruccion.Location = new Point(22, 50);

            txtExpresion = new TextBox();
            txtExpresion.Font = new Font("Segoe UI", 12);
            txtExpresion.Location = new Point(20, 80);
            txtExpresion.Size = new Size(500, 35);
            txtExpresion.Text = "3 * (9 - 3 * 4)";
            txtExpresion.BackColor = Color.White;
            txtExpresion.BorderStyle = BorderStyle.FixedSingle;
            txtExpresion.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) btnGenerar.PerformClick();
            };

            btnGenerar = new Button();
            btnGenerar.Text = "Generar Árbol";
            btnGenerar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnGenerar.Location = new Point(535, 78);
            btnGenerar.Size = new Size(150, 37);
            btnGenerar.BackColor = Color.FromArgb(0, 120, 215);
            btnGenerar.ForeColor = Color.White;
            btnGenerar.FlatStyle = FlatStyle.Flat;
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.Cursor = Cursors.Hand;
            btnGenerar.Click += BtnGenerar_Click;

            pnlArbol = new Panel();
            pnlArbol.Location = new Point(10, 130);
            pnlArbol.Size = new Size(860, 490);
            pnlArbol.BackColor = Color.White;
            pnlArbol.BorderStyle = BorderStyle.FixedSingle;
            pnlArbol.Paint += PnlArbol_Paint;
            pnlArbol.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                            | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblInstruccion);
            this.Controls.Add(txtExpresion);
            this.Controls.Add(btnGenerar);
            this.Controls.Add(pnlArbol);
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            string expr = txtExpresion.Text.Trim();
            if (string.IsNullOrEmpty(expr))
            {
                MessageBox.Show("Por favor ingresa una expresión.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var parser = new Parser(expr);
                raiz = parser.Parse();
                pnlArbol.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la expresión:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                raiz = null;
                pnlArbol.Invalidate();
            }
        }

        private void PnlArbol_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (raiz == null)
            {
                string msg = "Ingresa una expresión y presiona \"Generar Árbol\"";
                Font f = new Font("Segoe UI", 12);
                SizeF sz = g.MeasureString(msg, f);
                g.DrawString(msg, f, Brushes.LightGray,
                    (pnlArbol.Width - sz.Width) / 2,
                    (pnlArbol.Height - sz.Height) / 2);
                return;
            }

            int profundidad = raiz.Profundidad();
            int margen = 30;
            int anchoTotal = pnlArbol.Width - margen * 2;
            int alturaTotal = pnlArbol.Height - margen * 2;
            int alturaNivel = alturaTotal / Math.Max(profundidad, 1);

            AsignarPosiciones(raiz, margen, margen + anchoTotal,
                              margen + alturaNivel / 2, alturaNivel);
            DibujarArbol(g, raiz, 26);
        }

        private void AsignarPosiciones(NodoArbol nodo, int xMin, int xMax, int y, int alturaNivel)
        {
            if (nodo == null) return;
            nodo.X = (xMin + xMax) / 2;
            nodo.Y = y;
            AsignarPosiciones(nodo.Izquierdo, xMin, nodo.X, y + alturaNivel, alturaNivel);
            AsignarPosiciones(nodo.Derecho, nodo.X, xMax, y + alturaNivel, alturaNivel);
        }

        private void DibujarArbol(Graphics g, NodoArbol nodo, int radio)
        {
            if (nodo == null) return;

            Pen linea = new Pen(Color.FromArgb(180, 180, 180), 2);

            if (nodo.Izquierdo != null)
            {
                g.DrawLine(linea, nodo.X, nodo.Y, nodo.Izquierdo.X, nodo.Izquierdo.Y);
                DibujarArbol(g, nodo.Izquierdo, radio);
            }
            if (nodo.Derecho != null)
            {
                g.DrawLine(linea, nodo.X, nodo.Y, nodo.Derecho.X, nodo.Derecho.Y);
                DibujarArbol(g, nodo.Derecho, radio);
            }

            bool esOp = (nodo.Valor == "+" || nodo.Valor == "-"
                      || nodo.Valor == "*" || nodo.Valor == "/");

            Color fondoColor = esOp ? Color.FromArgb(210, 230, 255)
                                    : Color.FromArgb(210, 255, 225);
            Color bordeColor = esOp ? Color.FromArgb(0, 80, 180)
                                    : Color.FromArgb(0, 130, 60);

            Rectangle rect = new Rectangle(nodo.X - radio, nodo.Y - radio, radio * 2, radio * 2);
            g.FillEllipse(new SolidBrush(fondoColor), rect);
            g.DrawEllipse(new Pen(bordeColor, 2), rect);

            Font fnt = new Font("Segoe UI", 10, FontStyle.Bold);
            SizeF sz = g.MeasureString(nodo.Valor, fnt);
            g.DrawString(nodo.Valor, fnt, new SolidBrush(bordeColor),
                nodo.X - sz.Width / 2,
                nodo.Y - sz.Height / 2);
        }
    }
}
