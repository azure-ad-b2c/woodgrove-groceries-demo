namespace WoodGroveGroceriesWebApplication.Managers
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Models.Settings;

    public class PolicyManager
    {
        private readonly string _prefix;

        public PolicyManager(IConfiguration configuration)
        {
            var authOptions = AuthenticationCustomerOptions.Construct(configuration);
            _prefix = authOptions.PolicyPrefix;
        }

        // Sign-In policies
        public string SignInWithAgeGatingDevSample => $"{_prefix}signup_signin";

        public string SignInWithPersonalAccountLocalPhoneWithOtp => $"{_prefix}sign_in_personal_local_phone_withOtp";

        public string SignUpOrSignInWithPersonalAccountLocalEmail => $"{_prefix}sign_up_sign_in_personal_local_email";

        public string SignUpOrSignInWithPersonalAccountLocalEmailAndSocial => $"{_prefix}sign_up_sign_in_personal_local_email_and_social";

        public string SignUpOrSignInWithPersonalAccountLocalEmailSkipProgressiveProfile =>
            $"{_prefix}sign_up_sign_in_personal_local_email_skip_progressive_profile";

        public string SignUpOrSignInWithPersonalAccountLocalPhoneAndSocial => $"{_prefix}sign_up_sign_in_personal_local_phone_and_social";

        public string SignUpOrSignInWithPersonalAccountLocalUsernameAndSocial => $"{_prefix}sign_up_sign_in_personal_local_username_and_social";

        public string SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialSendInvitation =>
            $"{_prefix}sign_up_sign_in_personal_local_email_and_social_send_invitation";

        public string SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithPreAgeGating =>
            $"{_prefix}sign_up_sign_in_personal_local_username_and_social_with_pre_age_gating";

        public string SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithTotpMfa =>
            $"{_prefix}sign_up_sign_in_personal_local_email_and_social_totp";

        public string SignUpWithPersonalAccountLocalPhoneWithOtp => $"{_prefix}sign_up_personal_local_phone_withOtp";


        // Other policies
        public string DeleteAccount => $"{_prefix}delete_account";

        public string ForgotUserName => $"{_prefix}forgot_username";

        public string InviteExternalUser => $"{_prefix}invite_externalUser";

        public string LinkWithSocialAccounts => $"{_prefix}AccountLinking";

        public string MultifactorAuthentication => $"{_prefix}mfa";

        public string ProfileUpdateWithPersonalAccount => $"{_prefix}profile_update_personal";

        public string SignUpWithPersonalAccountLocalEmailAcceptInvitation => $"{_prefix}sign_up_personal_local_email_accept_invitation";

        public string PasswordReset => $"{_prefix}password_reset";

        public string StepUpTotp => $"{_prefix}stepup_totp";

        // Business customer policies

        public string SignUpOrSignInWithWorkAccount => $"{_prefix}sign_up_sign_in_work";

        public string ProfileUpdateWithWorkAccount => $"{_prefix}profile_update_work";

        public string SetStockerRole => $"{_prefix}profile_update_setrole_stocker";

        public string SetManagerRole => $"{_prefix}profile_update_setrole_manager";

        public string DefaultSignInPolicy => SignUpOrSignInWithPersonalAccountLocalEmailAndSocial;

        public List<string> CustomerPolicySetupList => new List<string>
        {
            SignInWithPersonalAccountLocalPhoneWithOtp,
            SignUpOrSignInWithPersonalAccountLocalEmail,
            SignUpOrSignInWithPersonalAccountLocalEmailAndSocial,
            SignUpOrSignInWithPersonalAccountLocalEmailSkipProgressiveProfile,
            SignUpOrSignInWithPersonalAccountLocalPhoneAndSocial,
            SignUpOrSignInWithPersonalAccountLocalUsernameAndSocial,
            SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialSendInvitation,
            SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithPreAgeGating,
            SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithTotpMfa,
            SignUpWithPersonalAccountLocalPhoneWithOtp,
            DeleteAccount,
            InviteExternalUser,
            LinkWithSocialAccounts,
            MultifactorAuthentication,
            ProfileUpdateWithPersonalAccount,
            SignUpWithPersonalAccountLocalEmailAcceptInvitation,
            StepUpTotp,
            SignUpOrSignInWithWorkAccount,
            ProfileUpdateWithWorkAccount,
            PasswordReset,
            DeleteAccount,
            SignUpOrSignInWithWorkAccount,
            ProfileUpdateWithWorkAccount,
            SetStockerRole,
            SetManagerRole
        };

        public List<string> BusinessCustomerPolicySetupList => new List<string>
        {
            DeleteAccount,
            SignUpOrSignInWithWorkAccount,
            ProfileUpdateWithWorkAccount,
            SetStockerRole,
            SetManagerRole
        };

        public Dictionary<string, string> PolicyList =>
            new Dictionary<string, string>
            {
                {"Local Only", SignUpOrSignInWithPersonalAccountLocalEmail},
                {"Local & Social", SignUpOrSignInWithPersonalAccountLocalEmailAndSocial},
                {"Social & Username - Inline Age Gating", SignUpOrSignInWithPersonalAccountLocalUsernameAndSocial},
                {"Social & Username - With Pre Age Gating", SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithPreAgeGating},
                {"Social & Phone", SignUpOrSignInWithPersonalAccountLocalPhoneAndSocial},
                {"Phone based OTP", SignInWithPersonalAccountLocalPhoneWithOtp},
                {"Social & Username - TOTP MFA", SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialWithTotpMfa}

                //{"Phone based OTP", SignUpWithPersonalAccountLocalPhoneWithOtp},
                //{"", SignUpOrSignInWithPersonalAccountLocalEmailSkipProgressiveProfile},
                //{"", SignUpOrSignInWithPersonalAccountLocalUsernameAndSocialSendInvitation},
            };
    }
}