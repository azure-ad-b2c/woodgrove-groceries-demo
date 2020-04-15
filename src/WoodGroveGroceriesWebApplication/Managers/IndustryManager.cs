namespace WoodGroveGroceriesWebApplication.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Entities;
    using Extensions;
    using Microsoft.AspNetCore.Http;
    using Models;
    using Services;

    public class IndustryManager
    {
        public enum Industry
        {
            Groceries,
            Banking,
            Wellness,
            None
        }

        private readonly HostService _host;

        private readonly Dictionary<Industry, IIndustry> _industryContainer = new Dictionary<Industry, IIndustry>();


        private Industry _currentIndustry = Industry.Groceries;

        public IndustryManager(HostService host)
        {
            _host = host;
        }

        public List<string> IndustryList => new List<string> {"Groceries", "Banking", "Wellness"};

        public Industry SetIndustry(HttpRequest request)
        {
            var configuredIndustry = request.Cookies[Constants.DemoCookies.IndustryKey].ToBase64Decode();

            Enum.TryParse(configuredIndustry, out Industry industry);

            _currentIndustry = industry;

            return industry;
        }

        public Industry GetIndustryIdentifier(string locale)
        {
            Industry industry;
            switch (locale)
            {
                case "en":
                case "es":
                case "fr":
                    industry = Industry.Groceries;
                    break;

                case "gn":
                case "co":
                case "as":
                    industry = Industry.Banking;
                    break;

                case "uz":
                case "ts":
                case "sw":
                    industry = Industry.Wellness;
                    break;

                default:
                    industry = Industry.None;
                    break;
            }

            return industry;
        }

        public IIndustry GetIndustry()
        {
            if (!_industryContainer.ContainsKey(_currentIndustry))
            {
                var newIndustry = _currentIndustry switch
                {
                    Industry.Banking => (IIndustry) new IndustryBanking(_host),
                    Industry.Groceries => new IndustryGroceries(_host),
                    Industry.Wellness => new IndustryHealthcare(_host),
                    _ => null
                };

                _industryContainer.Add(_currentIndustry, newIndustry);
            }

            return _industryContainer[_currentIndustry];
        }

        public IIndustry GetIndustry(string locale)
        {
            var industry = GetIndustryIdentifier(locale);
            if (industry == Industry.None)
            {
                return null;
            }

            if (!_industryContainer.ContainsKey(industry))
            {
                var newIndustry = industry switch
                {
                    Industry.Banking => (IIndustry) new IndustryBanking(_host),
                    Industry.Groceries => new IndustryGroceries(_host),
                    Industry.Wellness => new IndustryHealthcare(_host),
                    _ => null
                };

                _industryContainer.Add(industry, newIndustry);
            }

            return _industryContainer[industry];
        }
    }

    public enum LocalizedEmailUse
    {
        EmailVerification,
        InvitationFlow,
        SendUsernames
    }

    public interface IIndustry
    {
        string Title { get; }
        string IndividualCustomerAccountType { get; }

        string LandingPageMainText { get; }
        string LandingPageSubText { get; }
        string LandingPageTextStyling { get; }
        string LandingPageProductDisplayTitle { get; }

        string BackgroundImage { get; }

        string LoginPageIndividualCustomerImage { get; }
        string LoginPageIndividualCustomerTitle { get; }
        string LoginPageIndividualCustomerSubText { get; }
        string LoginPageIndividualCustomerAccountName { get; }

        string LoginPageBusinessCustomerImage { get; }
        string LoginPageBusinessCustomerTitle { get; }
        string LoginPageBusinessCustomerSubText { get; }
        string LoginPageBusinessCustomerAccountName { get; }

        string LoginPagePartnerImage { get; }
        string LoginPagePartnerTitle { get; }
        string LoginPagePartnerSubText { get; }
        string LoginPagePartnerAccountName { get; }

        string DefaultCustomerAuthBackground { get; }

        string CatalogItemsCategory1 { get; }
        string CatalogItemsCategory2 { get; }
        string CatalogItemsCategory3 { get; }

        string CatalogAddToCart { get; }

        string TrolleyCartEmpty { get; }
        string TrolleyProductHeading { get; }
        string TrolleyContinueShopping { get; }
        string TrolleyCompletePurchase { get; }

        string CheckoutMessage { get; }

        string CatalogHeader { get; }
        string PantryHeader { get; }
        string CartHeader { get; }
        string CartHeaderLogo { get; }
        string CartAlertIndicator { get; }
        bool ItemMultiPurchasable { get; }
        string GetCustomLocaleReplacement(string locale);

        CatalogItem ConvertItem(CatalogItem input);

        CustomLocaleValue GetLocalizedEmailString(LocalizedEmailUse use, string locale);
    }

    public class IndustryGroceries : IIndustry
    {
        private readonly HostService _host;

        private readonly Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>> _localizedEmails =
            new Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>>
            {
                {
                    LocalizedEmailUse.EmailVerification, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            "en", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Woodgrove Groceries account email verification code",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Thanks for verifying your {email} account!</a>.</p>Your code is: {code}<p><p>Regards,</p><p>The WoodGrove Groceries Team</p>"
                            }
                        },
                        {
                            "es", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Código de verificación de correo electrónico de la cuenta de Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>¡Gracias por verificar su cuenta de {email}!</a>.</p>Su código es: {code}<p><p>Saludos,</p><p>El equipo de WoodGrove Groceries</p>"
                            }
                        },
                        {
                            "fr", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Code de vérification de l'e-mail du compte Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>salut,</p><p>Merci d'avoir vérifié votre compte {email}!</a>.</p>Votre code est: {code}<p><p>Cordialement,</p><p>L'équipe WoodGrove Groceries</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.InvitationFlow, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            "en", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>{fromName} has invited you to register with Woodgrove Groceries</a>. </p><a href='{link}'>Accept Invitation</a><p><p>Regards,</p><p>the Woodgrove Groceries team</p>"
                            }
                        },
                        {
                            "es", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>{fromName} te ha invitado a registrarse con Woodgrove Groceries</a>. </p><a href='{link}'>Aceptar la invitacion</a><p><p>Saludos,</p><p>el equipo Woodgrove Groceries</p>"
                            }
                        },
                        {
                            "fr", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>salut,</p><p>{fromName} vous a invité à vous inscrire avec Woodgrove Groceries</a>. </p><a href='{link}'>Accepter l'invitation</a><p><p>Cordialement,</p><p>L'équipe Woodgrove Groceries</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.SendUsernames, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            "en", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Please find the credentials associated with the account number '{accountNumber}' <br/><br/> {userNameList}</p><p>Regards,</p><p>The Woodgrove Groceries team</p>"
                            }
                        },
                        {
                            "es", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>Por favor, encontrar las credenciales asociadas con el número de cuenta '{accountNumber}' <br/><br/> {userNameList}</p><p>Saludos,</p><p>El equipo Woodgrove Groceries</p>"
                            }
                        },
                        {
                            "fr", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Groceries",
                                EmailCodeHtmlContent =
                                    "<p>salut,</p><p>S'il vous plaît trouver les informations d'identification associées au numéro de compte '{accountNumber}' <br/><br/> {userNameList}</p><p>Cordialement,</p><p>L'équipe Woodgrove Groceries</p>"
                            }
                        }
                    }
                }
            };

        public IndustryGroceries(HostService host)
        {
            _host = host;
        }

        private Dictionary<string, string> customLocaleReplacements => new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("en", "en"),
                new KeyValuePair<string, string>("es", "es"),
                new KeyValuePair<string, string>("fr", "fr")
            });

        public string Title => "Woodgrove Groceries";

        public string LandingPageMainText => "Save Time";
        public string LandingPageSubText => "Let us do the grocery shopping for you";
        public string LandingPageTextStyling => "color: white;";
        public string LandingPageProductDisplayTitle => "SPECIALS";

        public string BackgroundImage => "../images/groceries/background.jpg";

        public string LoginPageIndividualCustomerImage => "../images/groceries/sign-in-b2c-personal.jpg";
        public string LoginPageIndividualCustomerTitle => "Individual customers";

        public string LoginPageIndividualCustomerSubText =>
            "Order your fresh groceries with WoodGrove Groceries and our friendly drivers will deliver your grocery shopping to your home door.";

        public string LoginPageIndividualCustomerAccountName => "personal account";

        public string LoginPageBusinessCustomerImage => "../images/groceries/sign-in-b2c-work.jpg";
        public string LoginPageBusinessCustomerTitle => "Business customers";

        public string LoginPageBusinessCustomerSubText =>
            "Order your fresh groceries with WoodGrove Groceries and our friendly drivers will deliver your grocery shopping to your office door.";

        public string LoginPageBusinessCustomerAccountName => "work account";

        public string LoginPagePartnerImage => "../images/groceries/sign-in-partner.jpg";
        public string LoginPagePartnerTitle => "Partners";

        public string LoginPagePartnerSubText =>
            "Manage your local produce in our inventory so we can deliver your fresh groceries to our customers.";

        public string LoginPagePartnerAccountName => "supplier account";

        public string DefaultCustomerAuthBackground => "images/groceries/background.jpg";

        public string CatalogItemsCategory1 => "Bakery";
        public string CatalogItemsCategory2 => "Dairy & Eggs";
        public string CatalogItemsCategory3 => "Fruit & Vegetables";

        public string TrolleyCartEmpty => "Your cart is empty";
        public string TrolleyProductHeading => "Product";
        public string TrolleyContinueShopping => "Continue shopping";
        public string TrolleyCompletePurchase => "Complete Purchase";

        public string CheckoutMessage => "Thank you for shopping at Woodgrove Groceries. Your order is being shipped.";

        public string CatalogAddToCart => "Add To Cart";

        public string CatalogHeader => "Catalog";
        public string CartHeader => "Cart";
        public string PantryHeader => "Pantry";
        public string CartHeaderLogo => "fa-shopping-cart";
        public string CartAlertIndicator => "images/alerticon.png";

        public bool ItemMultiPurchasable => true;

        public string IndividualCustomerAccountType => "Individual Customer";


        public string GetCustomLocaleReplacement(string locale)
        {
            if (customLocaleReplacements.ContainsKey(locale))
            {
                return customLocaleReplacements[locale];
            }

            return "en";
        }

        public CatalogItem ConvertItem(CatalogItem input)
        {
            switch (input.Id)
            {
                case "3696d034-deec-4ea3-8977-23558949b61c":
                    input.ProductName = "Apples";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/apples.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "730b79be-9a4a-4221-a2e1-248d4ed785a9":
                    input.ProductName = "Bananas";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/bananas.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "11ed2c06-88a6-4bf2-ae63-6f8638ed6044":
                    input.ProductName = "Oranges";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/oranges.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "a02af65f-507d-472d-be5f-e2f20fc94212":
                    input.ProductName = "Milk";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/milk-1056475.jpg";
                    input.ProductAllergyInfo = "Dairy";
                    break;

                case "db02856c-21b7-4510-aa27-6862620c326b":
                    input.ProductName = "Bulk Nuts";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/peanut-1328063.jpg";
                    input.ProductAllergyInfo = "Nuts";
                    break;

                case "85dc4475-a56f-4e64-9877-1717a9622279":
                    input.ProductName = "Bread";
                    input.ProductPictureUrl = $"{_host.HostName}/images/groceries/spelt-bread-2-1326657.jpg";
                    input.ProductAllergyInfo = "Gluten";
                    break;
            }

            return input;
        }

        public CustomLocaleValue GetLocalizedEmailString(LocalizedEmailUse use, string locale)
        {
            CustomLocaleValue item;

            if (_localizedEmails.ContainsKey(use) && _localizedEmails[use].ContainsKey(locale))
            {
                item = _localizedEmails[use][locale];
            }
            else
            {
                item = _localizedEmails.First().Value.First().Value;
            }

            return new CustomLocaleValue {EmailCodeSubject = item.EmailCodeSubject, EmailCodeHtmlContent = item.EmailCodeHtmlContent};
        }
    }

    public class IndustryBanking : IIndustry
    {
        private readonly HostService _host;

        private readonly Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>> _localizedEmails =
            new Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>>
            {
                {
                    LocalizedEmailUse.EmailVerification, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "gn", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Woodgrove Banking account email verification code",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Thanks for verifying your {email} account!</a>.</p>Your code is: {code}<p><p>Regards,</p><p>The WoodGrove Banking Team</p>"
                            }
                        },
                        {
                            // es
                            "co", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Código de verificación de correo electrónico de la cuenta bancaria de Woodgrove",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>¡Gracias por verificar su cuenta de {email}!</a>.</p>Su código es: {code}<p><p>Saludos,</p><p>El equipo de WoodGrove Banking</p>"
                            }
                        },
                        {
                            // fr
                            "as", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Code de vérification de l'e-mail du compte bancaire Woodgrove",
                                EmailCodeHtmlContent =
                                    "<p>salut,</p><p>Merci d'avoir vérifié votre compte {email}!</a>.</p>Votre code est: {code}<p><p>Cordialement,</p><p>L'équipe WoodGrove Banking</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.InvitationFlow, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "gn", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>{fromName} has invited you to register with Woodgrove Banking</a>. </p><a href='{link}'>Accept Invitation</a><p><p>Regards,</p><p>the Woodgrove Banking team</p>"
                            }
                        },
                        {
                            // es
                            "co", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>{fromName} te ha invitado a registrarse con Woodgrove Banking</a>. </p><a href='{link}'>Aceptar la invitacion</a><p><p>Saludos,</p><p>el equipo Woodgrove Banking</p>"
                            }
                        },
                        {
                            // fr
                            "as", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Salut,</p><p>{fromName} vous a invité à vous inscrire avec Woodgrove Banking</a>. </p><a href='{link}'>Accepter l'invitation</a><p><p>Cordialement,</p><p>L'équipe Woodgrove Banking</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.SendUsernames, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "gn", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Please find the credentials associated with the account number '{accountNumber}' <br/><br/> {userNameList}</p><p>Regards,</p><p>The Woodgrove Banking team</p>"
                            }
                        },
                        {
                            // es
                            "co", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>Por favor, encontrar las credenciales asociadas con el número de cuenta '{accountNumber}' <br/><br/> {userNameList}</p><p>Saludos,</p><p>El equipo Woodgrove Banking</p>"
                            }
                        },
                        {
                            // fr
                            "as", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Banking",
                                EmailCodeHtmlContent =
                                    "<p>Salut,</p><p>S'il vous plaît trouver les informations d'identification associées au numéro de compte '{accountNumber}' <br/><br/> {userNameList}</p><p>Cordialement,</p><p>L'équipe Woodgrove Banking</p>"
                            }
                        }
                    }
                }
            };

        public IndustryBanking(HostService host)
        {
            _host = host;
        }

        private Dictionary<string, string> customLocaleReplacements => new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("en", "gn"),
                new KeyValuePair<string, string>("es", "co"),
                new KeyValuePair<string, string>("fr", "as")
            });

        public string Title => "Woodgrove Banking";

        public string LandingPageMainText => "Invest Wisely";
        public string LandingPageSubText => "Let us manage your money";
        public string LandingPageTextStyling => "color: white;";
        public string LandingPageProductDisplayTitle => "FEATURED FINANCIAL PRODUCTS";

        public string BackgroundImage => "../images/banking/background.jpg";

        public string LoginPageIndividualCustomerImage => "../images/banking/personal-account.jpg";
        public string LoginPageIndividualCustomerTitle => "Personal Banking";
        public string LoginPageIndividualCustomerSubText => "Manage your personal bank and investment accounts.";
        public string LoginPageIndividualCustomerAccountName => "personal account";

        public string LoginPageBusinessCustomerImage => "../images/banking/business-account.jpg";
        public string LoginPageBusinessCustomerTitle => "Institutional Investing";
        public string LoginPageBusinessCustomerSubText => "Manage your institutional accounts and services.";
        public string LoginPageBusinessCustomerAccountName => "institution";

        public string LoginPagePartnerImage => "../images/banking/partner-account.jpg";
        public string LoginPagePartnerTitle => "Partners";
        public string LoginPagePartnerSubText => "Access regulatory reports and manage partner services.";
        public string LoginPagePartnerAccountName => "partner account";

        public string DefaultCustomerAuthBackground => "images/banking/background.jpg";

        public string CatalogItemsCategory1 => "Banking Services";
        public string CatalogItemsCategory2 => "Loans";
        public string CatalogItemsCategory3 => "Investments";

        public string TrolleyCartEmpty => "Your cart is empty";
        public string TrolleyProductHeading => "Product";
        public string TrolleyContinueShopping => "Continue shopping";
        public string TrolleyCompletePurchase => "Complete Purchase";

        public string CheckoutMessage => "Thank you for shopping at Woodgrove Banking.  An account broker will contact you shortly.";

        public string CatalogAddToCart => "Request Service";

        public string CatalogHeader => "Catalog";
        public string CartHeader => "Cart";
        public string PantryHeader => "Services";
        public string CartHeaderLogo => "";
        public string CartAlertIndicator => "images/select.png";
        public bool ItemMultiPurchasable => true;
        public string IndividualCustomerAccountType => "Individual Customer";

        public string GetCustomLocaleReplacement(string locale)
        {
            if (customLocaleReplacements.ContainsKey(locale))
            {
                return customLocaleReplacements[locale];
            }

            return "en";
        }

        public CatalogItem ConvertItem(CatalogItem input)
        {
            switch (input.Id)
            {
                case "3696d034-deec-4ea3-8977-23558949b61c":
                    input.ProductName = "Checking Account";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/checking.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "730b79be-9a4a-4221-a2e1-248d4ed785a9":
                    input.ProductName = "Savings Account";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/savings.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "11ed2c06-88a6-4bf2-ae63-6f8638ed6044":
                    input.ProductName = "Home Loan";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/home-loan.jpg";
                    input.ProductAllergyInfo = "Gluten";
                    break;

                case "a02af65f-507d-472d-be5f-e2f20fc94212":
                    input.ProductName = "Auto Loan";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/car-loan.jpg";
                    input.ProductAllergyInfo = "Nuts";
                    break;

                case "db02856c-21b7-4510-aa27-6862620c326b":
                    input.ProductName = "Investment Services";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/investments.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "85dc4475-a56f-4e64-9877-1717a9622279":
                    input.ProductName = "College Savings Accounts";
                    input.ProductPictureUrl = $"{_host.HostName}/images/banking/college-savings.jpg";
                    input.ProductAllergyInfo = "Dairy";
                    break;
            }

            return input;
        }

        public CustomLocaleValue GetLocalizedEmailString(LocalizedEmailUse use, string locale)
        {
            CustomLocaleValue item;

            if (_localizedEmails.ContainsKey(use) && _localizedEmails[use].ContainsKey(locale))
            {
                item = _localizedEmails[use][locale];
            }
            else
            {
                item = _localizedEmails.First().Value.First().Value;
            }

            return new CustomLocaleValue {EmailCodeSubject = item.EmailCodeSubject, EmailCodeHtmlContent = item.EmailCodeHtmlContent};
        }
    }

    public class IndustryHealthcare : IIndustry
    {
        private readonly HostService _host;

        private readonly Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>> _localizedEmails =
            new Dictionary<LocalizedEmailUse, Dictionary<string, CustomLocaleValue>>
            {
                {
                    LocalizedEmailUse.EmailVerification, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "uz", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Woodgrove Health Care account email verification code",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Thanks for verifying your {email} account!</a>.</p>Your code is: {code}<p><p>Regards,</p><p>The WoodGrove Wellness Team</p>"
                            }
                        },
                        {
                            // es
                            "ts", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Código de verificación de correo electrónico de la cuenta de Woodgrove Health Care",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>¡Gracias por verificar su cuenta de {email}!</a>.</p>Su código es: {code}<p><p>Saludos,</p><p>El equipo de WoodGrove Wellness</p>"
                            }
                        },
                        {
                            // fr
                            "sw", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Code de vérification de l'e-mail du compte Woodgrove Health Care",
                                EmailCodeHtmlContent =
                                    "<p>Salut,</p><p>Merci d'avoir vérifié votre compte {email}!</a>.</p>Votre code est: {code}<p><p>Cordialement,</p><p>L'équipe WoodGrove Wellness</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.InvitationFlow, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "uz", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>{fromName} has invited you to register with Woodgrove Wellness</a>. </p><a href='{link}'>Accept Invitation</a><p><p>Regards,</p><p>the Woodgrove Wellness team</p>"
                            }
                        },
                        {
                            // es
                            "ts", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>{fromName} te ha invitado a registrarse con Woodgrove Wellness</a>. </p><a href='{link}'>Aceptar la invitacion</a><p><p>Saludos,</p><p>el equipo Woodgrove Wellness</p>"
                            }
                        },
                        {
                            // fr
                            "sw", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Salut,</p><p>{fromName} vous a invité à vous inscrire avec Woodgrove Wellness</a>. </p><a href='{link}'>Accepter l'invitation</a><p><p>Cordialement,</p><p>L'équipe Woodgrove Wellness</p>"
                            }
                        }
                    }
                },
                {
                    LocalizedEmailUse.SendUsernames, new Dictionary<string, CustomLocaleValue>
                    {
                        {
                            // en
                            "uz", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Welcome to  Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Hi,</p><p>Please find the credentials associated with the account number '{accountNumber}' <br/><br/> {userNameList}</p><p>Regards,</p><p>The Woodgrove Wellness team</p>"
                            }
                        },
                        {
                            // es
                            "ts", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenido a Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Hola,</p><p>Por favor, encontrar las credenciales asociadas con el número de cuenta '{accountNumber}' <br/><br/> {userNameList}</p><p>Saludos,</p><p>El equipo Woodgrove Wellness</p>"
                            }
                        },
                        {
                            // fr
                            "sw", new CustomLocaleValue
                            {
                                EmailCodeSubject = "Bienvenue à Woodgrove Wellness",
                                EmailCodeHtmlContent =
                                    "<p>Salut,</p><p>S'il vous plaît trouver les informations d'identification associées au numéro de compte '{accountNumber}' <br/><br/> {userNameList}</p><p>Cordialement,</p><p>L'équipe Woodgrove Wellness</p>"
                            }
                        }
                    }
                }
            };

        public IndustryHealthcare(HostService host)
        {
            _host = host;
        }

        private Dictionary<string, string> customLocaleReplacements => new Dictionary<string, string>(
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("en", "uz"),
                new KeyValuePair<string, string>("es", "ts"),
                new KeyValuePair<string, string>("fr", "sw")
            });

        public string Title => "Woodgrove Wellness";

        public string LandingPageMainText => "Stay Healthy";
        public string LandingPageSubText => "Let us help you stay fit";
        public string LandingPageTextStyling => "color: white;";
        public string LandingPageProductDisplayTitle => "WELLNESS OFFERS";

        public string BackgroundImage => "../images/healthcare/background.jpg";

        public string LoginPageIndividualCustomerImage => "../images/healthcare/personal-account.jpg";
        public string LoginPageIndividualCustomerTitle => "Individual Member";
        public string LoginPageIndividualCustomerSubText => "Manage your individual wellness membership.";
        public string LoginPageIndividualCustomerAccountName => "individual account";

        public string LoginPageBusinessCustomerImage => "../images/healthcare/business-account.jpg";
        public string LoginPageBusinessCustomerTitle => "Group Member";
        public string LoginPageBusinessCustomerSubText => "Manage your group plan membership and offers.";
        public string LoginPageBusinessCustomerAccountName => "group account";

        public string LoginPagePartnerImage => "../images/healthcare/partner-account.jpg";
        public string LoginPagePartnerTitle => "Providers";
        public string LoginPagePartnerSubText => "Manage the services you offer to patients.";
        public string LoginPagePartnerAccountName => "provider account";

        public string DefaultCustomerAuthBackground => "images/healthcare/background.jpg";

        public string CatalogItemsCategory1 => "Fitness";
        public string CatalogItemsCategory2 => "Wellness";
        public string CatalogItemsCategory3 => "See a Provider";

        public string TrolleyCartEmpty => "You have not made any service selections";
        public string TrolleyProductHeading => "Wellness offerings";
        public string TrolleyContinueShopping => "Add More Services";
        public string TrolleyCompletePurchase => "Confirm Service Selections";

        public string CheckoutMessage => "Thank you for contacting Woodgrove Wellness.  A provider will contact you shortly.";

        public string CatalogAddToCart => "Add To Cart";

        public string CatalogHeader => "Catalog";
        public string CartHeader => "Cart";
        public string PantryHeader => "Pantry";
        public string CartHeaderLogo => "";
        public string CartAlertIndicator => "images/select.png";

        public bool ItemMultiPurchasable => false;

        public string IndividualCustomerAccountType => "Individual Member";

        public string GetCustomLocaleReplacement(string locale)
        {
            if (customLocaleReplacements.ContainsKey(locale))
            {
                return customLocaleReplacements[locale];
            }

            return "en";
        }

        public CatalogItem ConvertItem(CatalogItem input)
        {
            switch (input.Id)
            {
                case "3696d034-deec-4ea3-8977-23558949b61c":
                    input.ProductName = "Fitness Programs";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/fitness.jpg";
                    input.ProductAllergyInfo = "Dairy";
                    break;

                case "730b79be-9a4a-4221-a2e1-248d4ed785a9":
                    input.ProductName = "Nutritional Programs";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/nutrition.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "11ed2c06-88a6-4bf2-ae63-6f8638ed6044":
                    input.ProductName = "Natural Wellness";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/natural-wellness.jpg";
                    input.ProductAllergyInfo = null;
                    break;

                case "a02af65f-507d-472d-be5f-e2f20fc94212":
                    input.ProductName = "Stress Management";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/stress-management.jpg";
                    input.ProductAllergyInfo = "Nuts";
                    break;

                case "db02856c-21b7-4510-aa27-6862620c326b":
                    input.ProductName = "Physical Therapy";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/physical-therapy.jpg";
                    input.ProductAllergyInfo = "Gluten";
                    break;

                case "85dc4475-a56f-4e64-9877-1717a9622279":
                    input.ProductName = "Meet With a Doctor";
                    input.ProductPictureUrl = $"{_host.HostName}/images/healthcare/meet-doctor.jpg";
                    input.ProductAllergyInfo = null;
                    break;
            }

            return input;
        }

        public CustomLocaleValue GetLocalizedEmailString(LocalizedEmailUse use, string locale)
        {
            CustomLocaleValue item;

            if (_localizedEmails.ContainsKey(use) && _localizedEmails[use].ContainsKey(locale))
            {
                item = _localizedEmails[use][locale];
            }
            else
            {
                item = _localizedEmails.First().Value.First().Value;
            }

            return new CustomLocaleValue {EmailCodeSubject = item.EmailCodeSubject, EmailCodeHtmlContent = item.EmailCodeHtmlContent};
        }
    }
}