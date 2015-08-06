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

using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;

namespace SCSM.AzureAutomation.WPF.Connector
{
    /// <summary>
    /// Interaction logic for AzureAutomationWelcomePage.xaml
    /// </summary>
    public partial class AzureAutomationWelcomePage : WizardWelcomePageBase
    {
        private AzureAutomationWizardData azureautomationWizardData = null;

        public AzureAutomationWelcomePage(WizardData wizardData)
        {

            if (wizardData == null)
            {
                throw new ArgumentNullException("wizardData");
            }

            InitializeComponent();

            this.DataContext = wizardData;
            this.azureautomationWizardData = this.DataContext as AzureAutomationWizardData;

            this.Title = "Before You Begin";
            this.FinishButtonText = "Create";
        }

         
    }
}
