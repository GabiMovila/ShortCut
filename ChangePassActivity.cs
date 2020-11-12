using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ShortCut_Android_v4
{
    [Activity(Label = "ChangePassActivity", Theme = "@style/Login")]
    public class ChangePassActivity : Activity
    {
        #region Definitions and Initializations

        // Default sizes for hashing the password using PBKDF2 (salted password string)
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;
        // Variables for storing the hashed password and the salt
        private byte[] hashed_pass = new byte[HashByteSize];
        private byte[] salt = new byte[SaltByteSize];
        // String variables for storing the email and the password typed by the user
        private String userEmail;
        private String newPass;
        private String code;
        // String variables for storing the hashed passwords in the database
        private String string_hashed_newPass;
        // Data from the database
        private String realCode;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.change_pass_screen);
            // Create your application here

            #region Getting the user's input from the form fields
            // Get the email from user
            FindViewById<EditText>(Resource.Id.textIn_changePass_email).TextChanged += (o, e) =>
            {
                userEmail = FindViewById<EditText>(Resource.Id.textIn_changePass_email).Text;
            };
            // Get code from user
            FindViewById<EditText>(Resource.Id.textIn_changePass_code).TextChanged += (o, e) =>
            {
                code = FindViewById<EditText>(Resource.Id.textIn_changePass_code).Text;
            };
            // Get pass from user
            FindViewById<EditText>(Resource.Id.textIn_changePass_newPass).TextChanged += (o, e) =>
            {
                newPass = FindViewById<EditText>(Resource.Id.textIn_changePass_newPass).Text;
            };
            #endregion

            FindViewById<Button>(Resource.Id.buttonChangePass).Click += (o, e) =>
            {

                // Hardcoded realCode - se va lua din baza de date cand o avem
                realCode = "30333";
                // Codul se trimite prin sms si se intriduce de catre utilizator

                if (code == realCode)
                {
                    // Daca codurile corespund, se reseteaza parola

                    // 1. Generate a new code for future use
                    realCode = Convert.ToString(GenerateCode());

                    // 2. Hash the new password and prepair it for storing in the database
                    salt = GenerateSalt(SaltByteSize);
                    hashed_pass = ComputeHash(newPass, salt);
                    string_hashed_newPass = Convert.ToBase64String(hashed_pass);

                    // 3. Update pass, salt si code in bd

                    // 4. Message the user - success: Pop-up
                    Android.App.AlertDialog.Builder dialogChangePass = new AlertDialog.Builder(this);
                    AlertDialog alertChangePass = dialogChangePass.Create();
                    alertChangePass.SetTitle("Done!");
                    alertChangePass.SetMessage("The password has been changed.");
                    alertChangePass.SetButton("OK", (c, ev) =>
                    {
                        // Behaviour when ok is clicked
                    });
                    alertChangePass.Show();

                }
            };


            // Code for canceling the password changing procedure and going back to the login screen
            FindViewById<ImageButton>(Resource.Id.buttonCancel).Click += (o, e) =>
            StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
        }


        #region Methods for Code, Salt and HashedPass generation
        private int GenerateCode()
        {
            int code;
            Random generator = new Random();
            code = generator.Next(100000, 999999);
            return code;
        }

        internal static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
        {
            using (RNGCryptoServiceProvider saltGenerator = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[saltByteSize];
                saltGenerator.GetBytes(salt);
                return salt;
            }
        }
        
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