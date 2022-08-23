using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProyectoFinal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ProyectoFinal.Controllers
{
    public class HomeController : Controller
    {

        #region Atributos

        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        private string _strConexionSqlServer = string.Empty;

        #endregion


        #region Constructor

        public HomeController(ILogger<HomeController> logger, IConfiguration iConfig)
        {
            _logger = logger;
            _configuration = iConfig;
            _strConexionSqlServer = _configuration.GetSection("ConnectionStrings").GetSection("SqlServer").Value;
        }

        #endregion

        public IActionResult Index()
        {
            return View();
        }


        #region Acciones

        public IActionResult Login(LoginViewModel _model)
        {

            try
            {
                DataSet dt = new DataSet();
                
                // Conectamos a la BBDD para comprobar si el usuario existe

                SqlConnection con = new SqlConnection(_strConexionSqlServer);
                SqlCommand comando = new SqlCommand("SELECT * FROM USUARIOS WHERE LOWER(Dni) = '" + _model.Dni.Trim().ToLower() + "' AND LOWER(Clave) = '" + _model.Clave.Trim().ToLower() + "'", con);
                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                adaptador.Fill(dt,"Usuarios");

                if (dt.Tables["Usuarios"].Rows.Count == 1)
                {
                    LoginViewModel tmp = new LoginViewModel();

                    tmp.Id = Convert.ToInt32(dt.Tables["Usuarios"].Rows[0]["Id"].ToString());
                    tmp.Nombre = dt.Tables["Usuarios"].Rows[0]["Nombre"].ToString().Trim();
                    tmp.Apellidos = dt.Tables["Usuarios"].Rows[0]["Apellidos"].ToString().Trim();
                    tmp.NombreCompleto  = tmp.Nombre.Trim() + " " + tmp.Apellidos.Trim();

                    ISession session = HttpContext.Session;
                    session.SetString("UsuarioConectado", JsonConvert.SerializeObject(tmp));

                    return RedirectToAction("Home");
                }
                else
                {
                    ModelState.AddModelError("Dni", "Usuario & Password Incorrectos");
                    return View("Index", _model);
                }

            }
            catch(Exception ex)
            {
                // Aqui insertamos el error en la tabla de logs

                ModelState.AddModelError("Dni", "Error al validar el Usuario");
                return View("Index",_model);
            }
        }

        public IActionResult Home()
        {
            LoginViewModel tmp = new LoginViewModel();

            try
            {
                
                ISession session = HttpContext.Session;
                tmp = (session.GetString("UsuarioConectado") == null) ? new LoginViewModel() : JsonConvert.DeserializeObject<LoginViewModel>(session.GetString("UsuarioConectado"));

                return View(tmp);
            }
            catch(Exception ex)
            {
                // Aqui hay quer redireccionar a la vista de error
            }

            return View(tmp);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public string getData(int Id)
        {
            // Devolvemos un array con las facturas del año en curso

            DataSet dt = new DataSet();
            SqlConnection con = new SqlConnection(_strConexionSqlServer);
            SqlCommand comando = new SqlCommand("SELECT * FROM DOCUMENTOS WHERE UsuarioId =  '" + Id + "' ", con);
            SqlDataAdapter adaptador = new SqlDataAdapter(comando);
            adaptador.Fill(dt, "Facturas");

            string[] valores = { "0","0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

            foreach(DataRow row in dt.Tables["Facturas"].Rows)
            {
                int iMes = Convert.ToInt32(row["mes"].ToString());
                valores[iMes + 1] = row["Importe"].ToString().Replace(',','.');

            }

            return string.Join(", ", valores);
        }


        [HttpPost]
        public int _upload(IFormFile file)
        {
            int iValor = 0;

            try
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyToAsync(ms);

                        string strNombreFichero = file.FileName.Trim();
                        string strMesFactura = file.FileName.Trim().Split('-')[0];

                        byte[] btContenido = ms.ToArray();


                        string statement = "INSERT INTO DOCUMENTOS (MES,CONTENIDO,NOMBRE,IMPORTE,USUARIOID) VALUES (@FileMes,@FileContents,@FileName,125,'0')";

                        SqlConnection cn = new SqlConnection(_strConexionSqlServer);
                        using (var cmd = new SqlCommand() { Connection = cn, CommandText = statement })
                        {
                            cmd.Parameters.Add("@FileContents", SqlDbType.VarBinary, btContenido.Length).Value = btContenido;

                            cmd.Parameters.AddWithValue("@FileName", strNombreFichero);
                            cmd.Parameters.AddWithValue("@FileMes", strMesFactura);

                            try
                            {
                                cn.Open();

                                iValor = Convert.ToInt32(cmd.ExecuteScalar());

                            }
                            catch (Exception ex)
                            {
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return iValor;
        }

        [HttpPost]
        public int _editarDocumento(int Id)
        {
            int iValor = 0;

            try
            {
                SqlConnection con = new SqlConnection(_strConexionSqlServer);
                SqlCommand comando = new SqlCommand("UPDATE DOCUMENTOS SET USUARIOID = '"+ Id + "' WHERE ID = ( SELECT MAX(ID) FROM DOCUMENTOS)", con);

                con.Open();
                iValor = comando.ExecuteNonQuery();
                con.Close();

            }
            catch (Exception ex)
            {

            }

            return iValor;
        }

        #endregion
    }
}
