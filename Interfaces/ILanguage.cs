using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.Interfaces
{
    public interface ILanguage
    {
        public string loginToWebsite { get; }
        public string loginWithGoogle { get; }
        public string loginWithFacebook { get; }
        public string emailAdress { get; }
        public string password { get; }
        public string continueTxt { get; }
        public string haventHasAccount { get; }
        public string haveAccount { get; }
        public string signUp { get; }
        public string pickInterestCate { get; }
        public string confirm { get; }

    }
}
