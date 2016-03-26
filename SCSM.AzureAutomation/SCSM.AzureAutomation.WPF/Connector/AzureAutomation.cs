using SCSM.AzureAutomation.WPF.Connector;

using System;
using System.Collections.Generic;
using System.Windows;

using System.Resources;             //Has the ResourceManager class in it
using System.Drawing;               //Hsa the Bitmap class in it
using System.Windows.Media.Imaging; //Has the BitmapSource and BitmapSizeOptions classes in it
using System.Globalization;         //Has the CultureInfo class in it
using System.ComponentModel;        //Has the INotifyPropertyChanged class in it
using Microsoft.Win32;              //Has the RegistryKey class in it

using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

//Requires Microsoft.EnterpriseManagement.Core reference
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;

//Requires Microsoft.EnterpriseManagement.UI.WpfWizardFramework reference
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;

//Requires Microsoft.EnterpriseManagement.UI.SdkDataAccess reference
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;      // Has the ConsoleCommand class in it

//Requires Microsoft.EnterpriseManagement.UI.Foundation reference
using Microsoft.EnterpriseManagement.ConsoleFramework;
using System.Security;

namespace SCSM.AzureAutomation.WPF.Connector
{
    public class AzureAutomation : ConsoleCommand
    {
        public AzureAutomation() { 
        
        }

        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            if (parameters.Contains("Create")) 
            {
                WizardStory wizard = new WizardStory();

                //set the icon and title bar
                ResourceManager rm = new ResourceManager("SCSM.AzureAutomation.WPF.Connector.Resources", typeof(Resources).Assembly);
                Bitmap bitmap = (Bitmap)rm.GetObject("AzureAutomation2x24");
                IntPtr ptr = bitmap.GetHbitmap();
                BitmapSource bitmapsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                wizard.StoryImage = bitmapsource;
                wizard.WizardWindowTitle = "Create Azure Automation Connector";

                WizardData data = new AzureAutomationWizardData();
                wizard.WizardData = data;

                //add th pages
                wizard.AddLast(new WizardStep("Welcome", typeof(AzureAutomationWelcomePage), wizard.WizardData));
                wizard.AddLast(new WizardStep("Configuration", typeof(AzureAutomationConfigurationPage), wizard.WizardData));
                wizard.AddLast(new WizardStep("Summary", typeof(AzureAutomationSummaryPage), wizard.WizardData));
                wizard.AddLast(new WizardStep("Results", typeof(AzureAutomationResultPage), wizard.WizardData));

                //Create a wizard window and show it
                WizardWindow wizardwindow = new WizardWindow(wizard);
                // this is needed so that WinForms will pass messages on to the hosted WPF control
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(wizardwindow);
                wizardwindow.ShowDialog();

                //Update the view when done with the wizard so that the new connector shows
                if (data.WizardResult == WizardResult.Success)
                {
                    RequestViewRefresh();
                }
            }
            else if (parameters.Contains("Edit")) 
            {
                //Get the server name to connect to and connect to the server
                String strServerName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\System Center\\2010\\Service Manager\\Console\\User Settings", "SDKServiceMachine", "localhost").ToString();
                EnterpriseManagementGroup emg = new EnterpriseManagementGroup(strServerName);

                //Get the object using the selected node ID
                String strID = String.Empty;
                foreach (NavigationModelNodeBase node in nodes)
                {
                    strID = node["$Id$"].ToString();
                }
                EnterpriseManagementObject emoAAConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid(strID), ObjectQueryOptions.Default);

                //Create a new "wizard" (also used for property dialogs as in this case), set the title bar, create the data, and add the pages
                WizardStory wizard = new WizardStory();
                wizard.WizardWindowTitle = "Edit Azure Automation Connector";
                WizardData data = new AzureAutomationWizardData(emoAAConnector);
                wizard.WizardData = data;
                wizard.AddLast(new WizardStep("Configuration", typeof(AzureAutomationConfigurationPage), wizard.WizardData));

                //Show the property page
                PropertySheetDialog wizardWindow = new PropertySheetDialog(wizard);

                //Update the view when done so the new values are shown
                bool? dialogResult = wizardWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    RequestViewRefresh();
                }
            }
            else if (parameters.Contains("Delete") || parameters.Contains("Disable") || parameters.Contains("Enable"))
            {
                //Get the server name to connect to and create a connection
                String strServerName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\System Center\\2010\\Service Manager\\Console\\User Settings", "SDKServiceMachine", "localhost").ToString();
                EnterpriseManagementGroup emg = new EnterpriseManagementGroup(strServerName);

                //Get the object using the selected node ID
                String strID = String.Empty;
                foreach (NavigationModelNodeBase node in nodes)
                {
                    strID = node["$Id$"].ToString();
                }
                EnterpriseManagementObject emoAAConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid(strID), ObjectQueryOptions.Default);

                if (parameters.Contains("Delete"))
                {
                    //Remove the object from the database
                    IncrementalDiscoveryData idd = new IncrementalDiscoveryData();
                    idd.Remove(emoAAConnector);
                    idd.Commit(emg);
                }

                //Get the rule using the connector ID
                ManagementPack mpConnectors = emg.GetManagementPack("SCSM.AzureAutomation", "ac1fe0583b6c84af", new Version("1.0.0.0"));
                ManagementPack mpAAConnectorWorkflows = emg.GetManagementPack("SCSM.AzureAutomation.Workflows", null, new Version("1.0.0.0"));
                ManagementPackClass classAAConnector = mpConnectors.GetClass("SCSM.AzureAutomation.Connector");
                String strConnectorID = emoAAConnector[classAAConnector, "Id"].ToString();
                ManagementPackRule ruleConnector = mpAAConnectorWorkflows.GetRule(strConnectorID);

                //Update the Enabled property or delete as appropriate
                if (parameters.Contains("Delete"))
                {
                    ruleConnector.Status = ManagementPackElementStatus.PendingDelete;
                }
                else if (parameters.Contains("Disable"))
                {
                    emoAAConnector[classAAConnector, "Enabled"].Value = false;
                    ruleConnector.Enabled = ManagementPackMonitoringLevel.@false;
                    ruleConnector.Status = ManagementPackElementStatus.PendingUpdate;
                }
                else if (parameters.Contains("Enable"))
                {
                    emoAAConnector[classAAConnector, "Enabled"].Value = true;
                    ruleConnector.Enabled = ManagementPackMonitoringLevel.@true;
                    ruleConnector.Status = ManagementPackElementStatus.PendingUpdate;
                }

                //Commit the changes to the connector object and rule
                emoAAConnector.Commit();
                mpAAConnectorWorkflows.AcceptChanges();

                //Update the view when done so the item is either removed or the updated Enabled value shows
                RequestViewRefresh();
            }

        }
    }

    class AzureAutomationWizardData : WizardData, INotifyPropertyChanged 
    {
        private String strDisplayName = String.Empty;
        private String strSubscriptionID = String.Empty;
        private String strAutomationAccount = String.Empty;
        private String strRunAsAccountName = String.Empty;
        private String strResourceGroup = String.Empty;
        private String strRunasAccountPassword = String.Empty;
        private String strConnectorID = String.Empty;
        private Guid guidEnterpriseManagementObjectID = Guid.Empty;
        private String strErrorMessage = String.Empty;

        public String DisplayName
        {
            get 
            {
                return this.strDisplayName;
            }
            set
            {
                if (this.strDisplayName != value)
                {
                    this.strDisplayName = value;
                    this.NotifyPropertyChanged("DisplayName");
                }
            }
        }
        public String SubscriptionID 
        {
            get
            {
                return this.strSubscriptionID;
            }
            set 
            {
                if (this.strSubscriptionID != value) 
                {
                    this.strSubscriptionID = value;
                    this.NotifyPropertyChanged("SubscriptionID");
                }
            } 
        }
        public String ResourceGroup
        {
            get
            {
                return this.strResourceGroup;
            }
            set
            {
                if (this.strResourceGroup != value)
                {
                    this.strResourceGroup = value;
                    this.NotifyPropertyChanged("ResourceGroup");
                }
            }
        }
        public String AutomationAccount
        {
            get
            {
                return this.strAutomationAccount;
            }
            set
            {
                if (this.strAutomationAccount != value)
                {
                    this.strAutomationAccount = value;
                    this.NotifyPropertyChanged("AutomationAccount");
                }
            }
        }
        public String RunAsAccountName
        {
            get
            {
                return this.strRunAsAccountName;
            }
            set
            {
                if (this.strRunAsAccountName != value)
                {
                    this.strRunAsAccountName = value;
                    this.NotifyPropertyChanged("RunAsAccountName");
                }
            }
        }
        public String RunAsAccountPassword
        {
            get
            {
                return this.strRunasAccountPassword;
            }
            set
            {
                if (this.strRunasAccountPassword != value)
                {
                    this.strRunasAccountPassword = value;
                    this.NotifyPropertyChanged("RunAsAccountPassword");
                }
            }
        }
        public String ConnectorID
        {
            get
            {
                return this.strConnectorID;
            }
            set
            {
                if (this.strConnectorID != value)
                {
                    this.strConnectorID = value;
                    this.NotifyPropertyChanged("RuleName");
                }
            }
        }
        public Guid EnterpriseManagementObjectID
        {
            get
            {
                return this.guidEnterpriseManagementObjectID;
            }
            set
            {
                if (this.guidEnterpriseManagementObjectID != value)
                {
                    this.guidEnterpriseManagementObjectID = value;
                    this.NotifyPropertyChanged("EnterpriseManagementObjectID");
                }
            }
        }
        public String ErrorMessage
        {
            get
            {
                return this.strErrorMessage;
            }
            set
            {
                if (this.strErrorMessage != value)
                {
                    this.strErrorMessage = value;
                    this.NotifyPropertyChanged("ErrorMessage");
                }
            }
        }

        internal AzureAutomationWizardData() 
        {
        }

        internal AzureAutomationWizardData(EnterpriseManagementObject emoAAConnector)
        {
            //Get the server name to connect to
            String strServerName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\System Center\\2010\\Service Manager\\Console\\User Settings", "SDKServiceMachine", "localhost").ToString();

            //Connect to the server
            EnterpriseManagementGroup emg = new EnterpriseManagementGroup(strServerName);

            ManagementPack mpConnectors = emg.GetManagementPack("SCSM.AzureAutomation", "ac1fe0583b6c84af", new Version("1.0.0.0"));
            ManagementPackClass classAAConnector = mpConnectors.GetClass("SCSM.AzureAutomation.Connector");

            this.EnterpriseManagementObjectID = emoAAConnector.Id;
            this.DisplayName = emoAAConnector.DisplayName;
            this.ConnectorID = emoAAConnector[classAAConnector, "Id"].ToString();
            this.AutomationAccount = emoAAConnector[classAAConnector, "AutomationAccount"].ToString();
            this.SubscriptionID = emoAAConnector[classAAConnector, "SubscriptionID"].ToString();
            this.ResourceGroup = emoAAConnector[classAAConnector, "ResourceGroup"].ToString();
            this.RunAsAccountName = emoAAConnector[classAAConnector, "RunAsAccountName"].ToString();
            this.RunAsAccountPassword = emoAAConnector[classAAConnector, "RunAsAccountPassword"].ToString();
            
        }

        public override void AcceptChanges(WizardMode wizardMode)
        {
            if (wizardMode == WizardMode.PropertySheet)
            {
                this.UpdateConnectorInstance();
            }
            else
            {
                try
                {
                    this.CreateConnectorInstance();
                    this.WizardResult = WizardResult.Success;
                }
                catch (Exception ex)
                {
                    this.WizardResult = WizardResult.Failed;
                    this.ErrorMessage = ex.ToString();
                }
            }
        }

        private void CreateConnectorInstance() 
        {
            try
            {
                //Get the server name to connect to
                String strServerName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\System Center\\2010\\Service Manager\\Console\\User Settings", "SDKServiceMachine", "localhost").ToString();

                //Conneect to the server
                EnterpriseManagementGroup emg = new EnterpriseManagementGroup(strServerName);

                //Get the System MP so we can get the system key token and version so we can get other MPs using that info
                ManagementPack mpSystem = emg.ManagementPacks.GetManagementPack(SystemManagementPack.System);
                string strSystemKeyToken = mpSystem.KeyToken;
                ManagementPack mpSubscriptions = emg.GetManagementPack("Microsoft.SystemCenter.Subscriptions", strSystemKeyToken, new Version("1.0.0.0"));

                //Also get the System Center and Connector MPs - we'll need things from those MPs later
                ManagementPack mpSystemCenter = emg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
                ManagementPack mpConnectors = emg.GetManagementPack("SCSM.AzureAutomation", "ac1fe0583b6c84af", new Version("1.0.0.0"));
                ManagementPack mpAAConnectorWorkflows = emg.GetManagementPack("SCSM.AzureAutomation.Workflows", null, new Version("1.0.0.0"));

                //Get the AzureAutomationConnector class in the Connectors MP
                ManagementPackClass classAAConnector = mpConnectors.GetClass("SCSM.AzureAutomation.Connector");

                //Create a new CreatableEnterpriseManagementObject.  We'll set the properties on this and then post it to the database.
                EnterpriseManagementObject cemoAAConnector = new CreatableEnterpriseManagementObject(emg, classAAConnector);

                //Sytem.Entity properties
                cemoAAConnector[classAAConnector, "DisplayName"].Value = this.DisplayName;            //Required

                //Microsoft.SystemCenter.Connector properties
                //This is just a tricky way to create a unique ID which conforms to the syntax rules for MP element ID attribute values.
                String strConnectorID = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", "AAConnector", Guid.NewGuid().ToString("N"));
                cemoAAConnector[classAAConnector, "Id"].Value = strConnectorID;                       //Required; Key

                //System.LinkingFramework.DataSource properties            
                cemoAAConnector[classAAConnector, "DataProviderDisplayName"].Value = "Azure Automation Connector"; //Optional, shown in Connectors view
                cemoAAConnector[classAAConnector, "Enabled"].Value = true;                            //Optional, shown in Connectors view
                cemoAAConnector[classAAConnector, "Name"].Value = this.DisplayName;

                //SCSM.AzureAutomation.Connector properties
                cemoAAConnector[classAAConnector, "AutomationAccount"].Value = this.AutomationAccount;
                cemoAAConnector[classAAConnector, "SubscriptionID"].Value = this.SubscriptionID;
                cemoAAConnector[classAAConnector, "ResourceGroup"].Value = this.ResourceGroup;
                cemoAAConnector[classAAConnector, "RunAsAccountName"].Value = this.RunAsAccountName;
                cemoAAConnector[classAAConnector, "RunAsAccountPassword"].Value = this.RunAsAccountPassword;


                //Create Connector instance
                cemoAAConnector.Commit();

                //Now we need to create the CSV Connector rule...

                //Get the Scheduler data source module type from the System MP and the Windows Workflow Task Write Action Module Type from the Subscription MP
                ManagementPackDataSourceModuleType dsmtScheduler = (ManagementPackDataSourceModuleType)mpSystem.GetModuleType("System.Scheduler");
                ManagementPackWriteActionModuleType wamtWindowsWorkflowTaskWriteAction = (ManagementPackWriteActionModuleType)mpSubscriptions.GetModuleType("Microsoft.EnterpriseManagement.SystemCenter.Subscription.WindowsWorkflowTaskWriteAction");

                //Create a new rule for the CSV Connector in the Connectors MP.  Set the name of this rule to be the same as the connector instance ID so there is a pairing between them
                ManagementPackRule ruleAAConnector = new ManagementPackRule(mpAAConnectorWorkflows, strConnectorID);

                //Set the target and other properties of the rule
                ruleAAConnector.Target = mpSystemCenter.GetClass("Microsoft.SystemCenter.SubscriptionWorkflowTarget");

                //Create a new Data Source Module in the new CSV Connector rule
                ManagementPackDataSourceModule dsmSchedule = new ManagementPackDataSourceModule(ruleAAConnector, "DS1");

                //Set the configuration of the data source rule.  Pass in the frequency (number of minutes)
                dsmSchedule.Configuration =
                    "<Scheduler>" +
                        "<SimpleReccuringSchedule>" +
                            "<Interval Unit=\"Minutes\">60</Interval>" +
                        "</SimpleReccuringSchedule>" +
                        "<ExcludeDates />" +
                    "</Scheduler>";

                //Set the Schedule Data Source Module Type to the Simple Schedule Module Type from the System MP
                dsmSchedule.TypeID = dsmtScheduler;

                //Add the Scheduler Data Source to the Rule
                ruleAAConnector.DataSourceCollection.Add(dsmSchedule);

                //Now repeat essentially the same process for the Write Action module...

                //Create a new Write Action Module in the CSV Connector rule
                ManagementPackWriteActionModule wamAAConnector = new ManagementPackWriteActionModule(ruleAAConnector, "WA1");

                //Set the Configuration XML
                wamAAConnector.Configuration =
                    "<Subscription>" +
                        "<WindowsWorkflowConfiguration>" +
                            //Specify the Windows Workflow Foundation workflow Assembly name here
                            "<AssemblyName>SCSM.AzureAutomation.Workflows.AT</AssemblyName>" +
                            //Specify the type name of the workflow to call in the assembly here:
                            "<WorkflowTypeName>WorkflowAuthoring.RefreshConnector</WorkflowTypeName>" +
                            "<WorkflowParameters>" +
                                //Pass in the parameters here.  In this case the two parameters are the data file path and the mapping file path
                                "<WorkflowParameter Name=\"RefreshConnectorScript_ConnectorId\" Type=\"string\">" + strConnectorID + "</WorkflowParameter>" +
                            "</WorkflowParameters>" +
                            "<RetryExceptions />" +
                            "<RetryDelaySeconds>60</RetryDelaySeconds>" +
                            "<MaximumRunningTimeSeconds>300</MaximumRunningTimeSeconds>" +
                        "</WindowsWorkflowConfiguration>" +
                    "</Subscription>";

                //Set the module type of the module to be the Windows Workflow Task Write Action Module Type from the Subscriptions MP.
                wamAAConnector.TypeID = wamtWindowsWorkflowTaskWriteAction;

                //Add the Write Action Module to the rule
                ruleAAConnector.WriteActionCollection.Add(wamAAConnector);

                //Mark the rule as pending update
                ruleAAConnector.Status = ManagementPackElementStatus.PendingAdd; ;

                //Accept the rule changes which updates the database
                mpAAConnectorWorkflows.AcceptChanges();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.InnerException.Message);
            }

        }

        private void UpdateConnectorInstance() 
        {
            //Get the server name to connect to and connect
            String strServerName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\System Center\\2010\\Service Manager\\Console\\User Settings", "SDKServiceMachine", "localhost").ToString();
            EnterpriseManagementGroup emg = new EnterpriseManagementGroup(strServerName);

            //Get the Connectors MP and AA Connector Class
            ManagementPack mpConnectors = emg.GetManagementPack("SCSM.AzureAutomation", "ac1fe0583b6c84af", new Version("1.0.0.0"));
            ManagementPackClass classAAConnector = mpConnectors.GetClass("SCSM.AzureAutomation.Connector");

            //Get the Connector object using the object ID
            EnterpriseManagementObject emoAAConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>(this.EnterpriseManagementObjectID, ObjectQueryOptions.Default);

            //Set the property values to the new values
            emoAAConnector[classAAConnector, "DisplayName"].Value = this.DisplayName;
            emoAAConnector[classAAConnector, "AutomationAccount"].Value = this.AutomationAccount;
            emoAAConnector[classAAConnector, "SubscriptionID"].Value = this.SubscriptionID;
            emoAAConnector[classAAConnector, "ResourceGroup"].Value = this.ResourceGroup;
            emoAAConnector[classAAConnector, "RunAsAccountName"].Value = this.RunAsAccountName;
            emoAAConnector[classAAConnector, "RunAsAccountPassword"].Value = this.RunAsAccountPassword;
          
            

            //Update Connector instance
            emoAAConnector.Commit();
           
            mpConnectors.AcceptChanges();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public static class StringCipher
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
            
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
          
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
            
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            var rngCsp = new RNGCryptoServiceProvider();
           
           // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
            
            return randomBytes;
        }
    }

}