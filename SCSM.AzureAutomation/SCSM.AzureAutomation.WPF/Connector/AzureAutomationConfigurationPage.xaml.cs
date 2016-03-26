using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security;

using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.GenericForm;
using System.Runtime.InteropServices;

namespace SCSM.AzureAutomation.WPF.Connector
{
    /// <summary>
    /// Interaction logic for AzureAutomationConfigurationPage.xaml
    /// </summary>
    /// 

    
    public partial class AzureAutomationConfigurationPage : WizardRegularPageBase
    {
        private AzureAutomationWizardData azureautomationWizardData = null;

        public AzureAutomationConfigurationPage(WizardData wizardData)
        {
            if (wizardData == null)
            {
                throw new ArgumentNullException("wizardData");
            }

            InitializeComponent();

            this.DataContext = wizardData;
            this.azureautomationWizardData = this.DataContext as AzureAutomationWizardData;

            this.Title = "Configure the Azure Automation Connector";
            this.FinishButtonText = "Create";
            
        }

        private void WizardRegularPageBase_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                string encryptedString = StringCipher.Encrypt(((PasswordBox)sender).Password, this.azureautomationWizardData.RunAsAccountName);
                this.azureautomationWizardData.RunAsAccountPassword = encryptedString;
            }
        }

    }

}

    
            
