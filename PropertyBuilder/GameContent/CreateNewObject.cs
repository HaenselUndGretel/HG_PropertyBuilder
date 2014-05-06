using KryptonEngine.Manager;
using KryptonEngine.SceneManagement;
using Microsoft.Xna.Framework.Graphics;
using PropertyBuilder.GameContent.Scenes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PropertyBuilder.GameContent
{
  public partial class CreateNewObject : Form
  {
    public CreateNewObject()
    {
      InitializeComponent();
      Dictionary<String, Texture2D> images = TextureManager.Instance.GetAllEntities();

      foreach (String s in images.Keys)
        comboBox1.Items.Add(s);
      comboBox1.SelectedIndex = 0;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      PropertyScene.NewTextureName = comboBox1.SelectedItem.ToString(); 
      this.Close();
    }
  }
}
