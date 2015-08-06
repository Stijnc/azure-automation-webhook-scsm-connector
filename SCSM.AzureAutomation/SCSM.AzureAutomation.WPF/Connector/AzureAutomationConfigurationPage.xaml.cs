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
using System.Runtime.InteropServices;

namespace SCSM.AzureAutomation.WPF.Connector
{
    /// <summary>
    /// Interaction logic for AzureAutomationConfigurationPage.xaml
    /// </summary>
    /// 
    public static class PasswordBoxAttachedProperties
    {

    }

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
            //Cast the 'sender' to a PasswordBox
            PasswordBox pBox = sender as PasswordBox;

            //Set this "EncryptedPassword" dependency property to the "SecurePassword"
            //of the PasswordBox.
            PasswordBoxAttachedProperties.SetEncryptedPassword(pBox, pBox.SecurePassword);
        }

        public static SecureString GetEncryptedPassword(DependencyObject obj)
        {
            return (SecureString)obj.GetValue(EncryptedPasswordProperty);
        }

        public static void SetEncryptedPassword(DependencyObject obj, SecureString value)
        {
            obj.SetValue(EncryptedPasswordProperty, value);
        }

        // Using a DependencyProperty as the backing store for EncryptedPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EncryptedPasswordProperty =
            DependencyProperty.RegisterAttached("EncryptedPassword", typeof(SecureString), typeof(PasswordBoxAttachedProperties));
    }
}

    
            
