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

using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.FormsInfra;
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common;

namespace SCSM.AzureAutomation.WPF.ConfigItem
{
    /// <summary>
    /// Interaction logic for AzureWebhookForm.xaml
    /// </summary>
    public partial class AzureWebhookForm : UserControl
    {
        public AzureWebhookForm()
        {
            InitializeComponent();

            RelatedItemsPane relatedItemsPane = new RelatedItemsPane(new ConfigItemRelatedItemsConfiguration());
            tabItemRelItems.Content = relatedItemsPane;
        }

        private void ProjectForm_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.AddHandler(FormEvents.PreviewSubmitEvent,
                new EventHandler<PreviewFormCommandEventArgs>(this.OnPreviewSubmit));
        }

        private void OnPreviewSubmit(object sender, PreviewFormCommandEventArgs e)
        {
            IDataItem itemWebhook = this.DataContext as IDataItem;
            string displayName = itemWebhook["DisplayName"].ToString();
            itemWebhook["DisplayName"] = displayName;

        }
        private void expanderMain_OnLoaded(object sender, RoutedEventArgs e)
        {
            HeaderedContentControl headeredContentControl = new HeaderedContentControl
            {
                OverridesDefaultStyle = true,
                Foreground = Brushes.Black,
                Header = "Azure Webhook Identity"
            };
            this.ExpanderMain.Header = headeredContentControl;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            createNewAzureConnector();
        }

        private void createNewAzureConnector()
        {
            throw new NotImplementedException();
        }
    }
}
