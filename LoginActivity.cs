using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ShortCut_Android_v4
{
    [Activity(Label = "LoginActivity", Theme = "@style/Login")]
    public class LoginActivity : Activity
    {
        #region Definitions and Initializations

        // Default sizes for hashing the password using PBKDF2 (salted password string)
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;

        // String variables for storing the email and the password typed by the user
        private String user_email;
        private String password;

        // Variables for storing the hashed password and the salt
        private byte[] hashed_pass = new byte[HashByteSize];
        private byte[] salt = new byte[SaltByteSize]; 
        // se ia din baza de date pe baza emailului ca sa se poata compara parolele hashed
        // fiecare utilizator are salt propriu generat la inregistrare si stocat in baza de date impreuna cu parola hashed 

        // variabile mock care ar trebui luate din baza de date pentru comparare cu cele date ca input si logare
        private String user_mock;
        private byte[] pass_mock;

        // String variables for storing the hashed passwords in the database
        private String string_hashed_pass;
        private String string_hashed_mock_pass;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login_screen);
            // Create your application here

            #region Code for redirecting in case of forgotten password or new user
            // Redirecting to registration form in case of new user
            FindViewById<ImageButton>(Resource.Id.buttonNeedAccount).Click += (o, e) =>
            StartActivity(new Intent(Application.Context, typeof(RegisterActivity)));

            // Redirecting to change password in case of forgot password
            FindViewById<ImageButton>(Resource.Id.buttonForgotPass).Click += (o, e) =>
            StartActivity(new Intent(Application.Context, typeof(ChangePassActivity)));
            #endregion

            #region Getting the user's input from the form fields
            // Get the email from user
            FindViewById<EditText>(Resource.Id.textIn_login_email).TextChanged += (o, e) =>
            {
                user_email = FindViewById<EditText>(Resource.Id.textIn_login_email).Text;
            };

            // Get pass from user
            FindViewById<EditText>(Resource.Id.textIn_login_pass).TextChanged += (o, e) =>
            {
                password = FindViewById<EditText>(Resource.Id.textIn_login_pass).Text;
            };
            #endregion

            #region Code for trying to authenticate when the login button is pressed

            FindViewById<Button>(Resource.Id.buttonLogin).Click += async (o, e) =>
            {
                #region Harcodari pentru debugging si testing pana implementam baza de date
                // Generarea unui salt "hardcodat" pentru a putea testa functionalitatea
                var hardcodedSalt = new byte[SaltByteSize];
                for (int i = 0; i < hardcodedSalt.Length; i++)
                {
                    hardcodedSalt[i] = 0x05;
                }
                string hardcoded_pass = "ardeal";
                #endregion

                // Username from database
                user_mock = "catalin@fabricadelapte.ro";


                if (user_mock != user_email)
                {
                    // Daca nu gaseste userul in baza de date
                    #region Mesaj daca userul nu-i in baza de date
                    // Pop-up message for wrong username
                    Android.App.AlertDialog.Builder dialogUserName = new AlertDialog.Builder(this);
                    AlertDialog alertUserName = dialogUserName.Create();
                    alertUserName.SetTitle("Ba fra.");
                    alertUserName.SetMessage("Vezi ca nu-i bun emailul");
                    alertUserName.SetButton("OK", (c, ev) =>
                    {
                        // what happens when ok is tapped
                    });
                    alertUserName.Show();
                    #endregion
                }
                else if (user_mock == user_email)
                {
                    // Get the salt from the database based on user's email]
                    salt = hardcodedSalt; // 24 bytes - ar trebui luat din baza de date cand o avem

                    // Taking the hashed password stored in the database for the current user
                    pass_mock = ComputeHash(hardcoded_pass, salt); // Hardcoded computation of the "stored" hashed password

                    // Computing the hash based on the input from the current user
                    hashed_pass = ComputeHash(password, salt);

                    // Converting both passwords to a string
                    string_hashed_pass = Convert.ToBase64String(hashed_pass);
                    string_hashed_mock_pass = Convert.ToBase64String(pass_mock);

                    // Authentication or message in case of wrong username / password
                    if (string_hashed_pass == string_hashed_mock_pass)
                    {
                        StartActivity(new Intent(Application.Context, typeof(CustomerHomeActivity))); // merge la pagina principala 
                    }
                    else if (pass_mock != hashed_pass)
                    {
                        // Pop-up message for wrong password
                        Android.App.AlertDialog.Builder dialogPass = new AlertDialog.Builder(this);
                        AlertDialog alertPass = dialogPass.Create();
                        alertPass.SetTitle("Aolo!");
                        alertPass.SetMessage("Nu-i buna parola.");
                        alertPass.SetButton("OK", (c, ev) =>
                        {
                            // what happens when ok is tapped
                        });
                        alertPass.Show();

                        // Debugging
                        // StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                        // merge la pagina principala - inseamna ca nu functioneaza sistemul de hashing la login
                        // hashuri diferite pentru aceeasi parola

                    }
                    else
                    {

                    }
                }

                #region Old if with both username and pass - commented
                //if ((user_mock == user_email) && (pass_mock == hashed_pass))
                //{
                //    StartActivity(new Intent(Application.Context, typeof(MainActivity))); // merge la pagina principala 
                //}
                //else if ((user_mock == user_email) && (pass_mock != hashed_pass))
                //{
                //    // Pop-up message for wrong password
                //    StartActivity(new Intent(Application.Context, typeof(MainActivity))); // merge la pagina principala 
                //}
                //else
                //{
                //    // Pop-up message for wrong username
                //}
                #endregion

            };

            #endregion
        }


        #region Internal method for computing the Hashed password based on the input from the user and the user's salt
        internal static byte[] ComputeHash(string password, byte[] salt, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
        {
            using (Rfc2898DeriveBytes hashGenerator = new Rfc2898DeriveBytes(password, salt))
            {
                hashGenerator.IterationCount = iterations;
                return hashGenerator.GetBytes(hashByteSize);
            }
        }
        #endregion
    }
}