using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ShortCut_Android_v4
{
    [Activity(Label = "RegisterActivity", Theme = "@style/Login")]
    public class RegisterActivity : Activity
    {

        #region Definitions and Initializations

        // Default sizes for hashing the password using PBKDF2 (salted password string)
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;
        // User input variables for handling the data inputed in the form
        private String fullName;
        private String userEmail;
        private String phoneNumber;
        private String pass;
        private String passConfirm;
        private String accountType;
        // Variables for handling the hashed password 
        private byte[] hashed_pass = new byte[HashByteSize];
        private byte[] salt = new byte[SaltByteSize];


        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Load the corresponding screen
            SetContentView(Resource.Layout.register_screen);

            #region Has Account redirect button
            // Redirecting the user in case he has an account
            FindViewById<ImageButton>(Resource.Id.button_hasAccount).Click += (o, e) =>
            StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
            #endregion

            #region Getting the user input from the form fields

            // Get the full name from user
            FindViewById<EditText>(Resource.Id.textIn_reg_name).TextChanged += (o, e) =>
            {
                fullName = FindViewById<EditText>(Resource.Id.textIn_reg_name).Text;
            };
            // Get email from user
            FindViewById<EditText>(Resource.Id.textIn_reg_email).TextChanged += (o, e) =>
            {
                userEmail = FindViewById<EditText>(Resource.Id.textIn_reg_email).Text;
            };
            // Get phone number from user
            FindViewById<EditText>(Resource.Id.textIn_reg_phone).TextChanged += (o, e) =>
            {
                userEmail = FindViewById<EditText>(Resource.Id.textIn_reg_phone).Text;
            };
            // Get the pass from user
            FindViewById<EditText>(Resource.Id.textIn_reg_pass).TextChanged += (o, e) =>
            {
                pass = FindViewById<EditText>(Resource.Id.textIn_reg_pass).Text;
            };
            // Get pass confirmation from user
            FindViewById<EditText>(Resource.Id.textIn_reg_conf_pass).TextChanged += (o, e) =>
            {
                passConfirm = FindViewById<EditText>(Resource.Id.textIn_reg_conf_pass).Text;
            };
            // Get the account type from user - radio button
            if (FindViewById<RadioButton>(Resource.Id.radioButton_barber).Checked)
            {
                accountType = "Barber";
            }
            else if (FindViewById<RadioButton>(Resource.Id.radioButton_customer).Checked)
            {
                accountType = "Customer";
            }

            #endregion

            #region Handling the registration procedure when the register button is presssed

            FindViewById<Button>(Resource.Id.buttonRegister).Click += (o, e) =>
            {
                //calaculare salt
                salt = GenerateSalt(SaltByteSize);
                // adaugare salt la parola si hasuire, parola asta si saltul trebuie adaugate in baza de date nu cea introdusa de utilizator
                hashed_pass = ComputeHash(pass, salt);
                // Cata: De mentionat ii ca trebe convertite la string inainte ca daca nu crapa si nu se pot folosi
                // Ar trebui sa fie cam asa:  string_hashed_pass = Convert.ToBase64String(hashed_pass);

                //cumva ar trebui verificat cu un query daca exista emailul si numele utilizatorului deja in baza de date
                //daca da sa se afiseze ca au fost folosite deja
                // Ceva gen dar pe toata baza de date:
                string taken_email = "catalin@fabricadelapte.ro";
                if (userEmail == taken_email)
                {
                    Android.App.AlertDialog.Builder dialogEmail = new AlertDialog.Builder(this);
                    AlertDialog alertEmail = dialogEmail.Create();
                    alertEmail.SetTitle("There is a problem.");
                    alertEmail.SetMessage("This email address is already taken.");
                    alertEmail.SetButton("OK", (c, ev) =>
                    {
                        // Relanseaza activitatea dar daca nu il puneam mergea inapoi la login
                        // StartActivity(new Intent(Application.Context, typeof(RegisterActivity)));
                    });
                    alertEmail.Show();
                }

                // Verificam daca parolele coincid
                if (pass == passConfirm)
                {
                    // 1. Compute hash for the passwords

                    // 2. Insert the data inputed by the user into the database

                    // 3. Pop-up registration successful
                    Android.App.AlertDialog.Builder dialogRegister = new AlertDialog.Builder(this);
                    AlertDialog alertRegister = dialogRegister.Create();
                    alertRegister.SetTitle("Done.");
                    alertRegister.SetMessage("You are now registered.");
                    alertRegister.SetButton("OK", (c, ev) =>
                    {
                        // 4. Redirect to the login page.
                        StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
                    });
                    alertRegister.Show();

                    #region din versiune 3.69
                    //adaugare in baza de date, afisare popup inregistrare cu succes si redirectionare la pagina de login
                    // StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
                    #endregion
                }
                else
                {
                    //parolele nu coincid
                    Android.App.AlertDialog.Builder dialogDiffPass = new AlertDialog.Builder(this);
                    AlertDialog alertDiffPass = dialogDiffPass.Create();
                    alertDiffPass.SetTitle("There is a problem.");
                    alertDiffPass.SetMessage("Your passwords don't match.");
                    alertDiffPass.SetButton("OK", (c, ev) =>
                    {
                        // What happens when ok is pressed
                    });
                    alertDiffPass.Show();
                }
            };

            #endregion

        }

        #region Definition for the GenerateSalt and ComputeHash methods used

        // Genereaza un sir random de biti care va fi adaugat la parola ca sa nu fie parole la fel, 
        // trebuie generat pentru fiecare user cand se inregistreaza, stocat in baza de date impreuna cu parola hashed, 
        // cand userul incearca sa se inregistreze se adauga hashul lui la parola care a introduso, se introduce in functia de hash si se compara cu cea din baza de date
        internal static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
        {
            using (RNGCryptoServiceProvider saltGenerator = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[saltByteSize];
                saltGenerator.GetBytes(salt);
                return salt;
            }
        }
        // functia de hash, le-am luat de pe https://janaks.com.np/password-hashing-in-csharp/ , mi se pare destul de ok
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