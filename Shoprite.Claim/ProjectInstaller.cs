using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ShopriteClaimsDownloadService
{
    [RunInstaller(true)]
    public partial class ShopriteClaimsProjectInstaller : System.Configuration.Install.Installer
    {
        public ShopriteClaimsProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
