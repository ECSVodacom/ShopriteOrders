namespace ShopriteBizlinkSender
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BizlinkServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.BizlinkServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // BizlinkServiceProcessInstaller
            // 
            this.BizlinkServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.BizlinkServiceProcessInstaller.Password = null;
            this.BizlinkServiceProcessInstaller.Username = null;
            // 
            // BizlinkServiceInstaller
            // 
            this.BizlinkServiceInstaller.Description = "Polls the Shoprite database for orders to send to Bislink";
            this.BizlinkServiceInstaller.DisplayName = "Shoprite Bizlink Sender Service";
            this.BizlinkServiceInstaller.ServiceName = "Service1";
            this.BizlinkServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.BizlinkServiceProcessInstaller,
            this.BizlinkServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller BizlinkServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller BizlinkServiceInstaller;
    }
}