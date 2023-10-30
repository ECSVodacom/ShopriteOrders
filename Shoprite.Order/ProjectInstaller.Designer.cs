namespace ShopriteOrderDownloadService
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
            this.ShopriteProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ShopriteServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ShopriteProcessInstaller
            // 
            this.ShopriteProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.ShopriteProcessInstaller.Password = null;
            this.ShopriteProcessInstaller.Username = null;
            // 
            // ShopriteServiceInstaller
            // 
            this.ShopriteServiceInstaller.Description = "Downloads Shoprite orders for vendors registered in the Shoprite database";
            this.ShopriteServiceInstaller.DisplayName = "Shoprite Order Downloader";
            this.ShopriteServiceInstaller.ServiceName = "ShopriteDownloadService";
            this.ShopriteServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ShopriteProcessInstaller,
            this.ShopriteServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ShopriteProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ShopriteServiceInstaller;
    }
}