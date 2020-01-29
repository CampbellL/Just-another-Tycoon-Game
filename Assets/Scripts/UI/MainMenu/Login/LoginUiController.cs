using System;
using Api;
using Game;
using Newtonsoft.Json;
using UI.Xml;
using UnityEngine.SceneManagement;
using Zenject;

namespace UI.MainMenu.Login
{
    internal sealed class LoginUiController : XmlLayoutController
    {
        private XmlElementReference<XmlElement> _loginWindowReference;
        private XmlElementReference<XmlElement> _registerWindowReference;
        private XmlElementReference<XmlElement> _mainWindowReference;
        private XmlElementReference<XmlElement> _userLoginReference;
        private ApiHandler _apiHandler;

        [Inject]
        public void PopulateDependencies(ApiHandler apiHandler)
        {
            this._apiHandler = apiHandler;
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            this._loginWindowReference = _loginWindowReference ?? this.XmlElementReference<XmlElement>("LoginWindow");
            this._registerWindowReference =
                _registerWindowReference ?? this.XmlElementReference<XmlElement>("RegisterWindow");
            this._mainWindowReference = _mainWindowReference ?? this.XmlElementReference<XmlElement>("MainMenu");
            this._userLoginReference = _userLoginReference ?? this.XmlElementReference<XmlElement>("userLogin");
            this.RefreshLoginStatus();
        }

        private void RefreshLoginStatus()
        {
            if (PlayerState.GetUserName() != null)
            {
                this._userLoginReference.element.SetAttribute("text","Welcome back " + PlayerState.GetUserName());
            }
            else
            {
                this._userLoginReference.element.SetAttribute("text","Not logged in");
            }
            this._userLoginReference.element.ApplyAttributes();
        }
        
        public void Options()
        {
        }

        public void Play()
        {
            if (PlayerState.GetAccessToken() != null)
            {
                SceneManager.LoadScene("Game");
            }
            else
            {
                this._mainWindowReference.element.Hide();
                this._loginWindowReference.element.Show();
            }
        }

        public void Logout()
        {
            PlayerState.DeleteAccessToken();
            RefreshLoginStatus();
        }
        
        public void Login()
        {
            var accessToken = new {access_token = ""};
            var formData = this.xmlLayout.GetFormData();
            LoginDto loginDto = new LoginDto()
            {
                username = formData["username"],
                password = formData["password"]
            };
            try
            {
                var token = JsonConvert.DeserializeAnonymousType(this._apiHandler.Login(loginDto), accessToken);
                PlayerState.SaveAccessToken(token.access_token);
                SceneManager.LoadScene("Game");
            }
            catch (UnauthorizedAccessException e)
            {
                XmlElement status =
                    this._loginWindowReference.element.GetElementByInternalId<XmlElement>("loginStatus");
                status.SetAttribute("text", "Login Failed");
                status.ApplyAttributes();
            }
        }

        public void Register()
        {
            var formData = this.xmlLayout.GetFormData();
            RegisterDto registerDto = new RegisterDto()
            {
                firstName = formData["FirstName"],
                lastName = formData["LastName"],
                username = formData["registerUsername"],
                password = formData["registerPassword"],
                emailAddress = formData["EmailAddress"],
            };
            try
            {
                this._apiHandler.Register(registerDto);
                XmlElement status = this._registerWindowReference.element.GetElementByInternalId<XmlElement>("registerStatus");
                status.SetAttribute("text","Registration Successful");
                status.ApplyAttributes();
            }
            catch (Exception e)
            {
                XmlElement status = this._registerWindowReference.element.GetElementByInternalId<XmlElement>("registerStatus");
                status.SetAttribute("text","Registration Failed");
                status.ApplyAttributes();
                Console.WriteLine(e);
                throw;
            }
        }

        public void ChangeToRegister()
        {
            this._loginWindowReference.element.Hide();
            this._registerWindowReference.element.Show();
        }

        public void ChangeToLogin()
        {
            this._loginWindowReference.element.Show();
            this._registerWindowReference.element.Hide();
        }
    }
}