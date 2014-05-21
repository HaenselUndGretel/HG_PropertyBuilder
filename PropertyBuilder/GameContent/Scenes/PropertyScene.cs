using KryptonEngine;
using KryptonEngine.Controls;
using KryptonEngine.Manager;
using KryptonEngine.SceneManagement;
using KryptonEngine.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.IO;

namespace PropertyBuilder.GameContent.Scenes
{
  public enum RectangleArt 
  {
    Action,
    Collision,
    StartPos1,
    StartPos2,
    Z,
    Delete
  }

  public class PropertyScene : Scene
  {
      #region Properties

      protected InteractiveObject interactivObject;

      protected CreateNewObject createForm;
      public static String NewTextureName;

      protected bool DrawActionRectangle = true;
      protected bool DrawCollisionRectangle = true;
      protected bool DrawPosition = true;
      protected bool DrawZ = true;
      protected bool IsDrawingRectangle = false;

      protected SpriteFont font;

      protected RectangleArt art = RectangleArt.Action;

      protected Vector2 mRectanglePos1 = Vector2.Zero;
      protected Vector2 mRectanglePos2 = Vector2.Zero;

      protected Rectangle tmpRectangle;

      protected Vector2 mRectangleSelectPos2;
      protected Vector2 mRectangleSelectPos1;

      protected bool isInCollision = false;
      protected bool isInAction = false;

      protected KeyboardState mKsCurrent;
      protected KeyboardState mKSLast;

      #endregion

      #region Getter & Setter

      #endregion

      #region Constructor

      public PropertyScene(String pSceneName)
        : base(pSceneName)
      {
        interactivObject = new InteractiveObject();
        
      }
      #endregion

      #region Override Methods

      public override void Initialize()
      {
        mClearColor = Color.Black;
      }

      public override void LoadContent()
      {
		  font = FontManager.Instance.GetElementByString("font");
		  interactivObject = new InteractiveObject();
        //interactivObject = InteractiveObjectDataManager.Instance.GetElementByString("Hansel");
        //interactivObject.Position = new Vector2((EngineSettings.VirtualResWidth / 2 - interactivObject.Texture.Width / 2), (EngineSettings.VirtualResHeight / 2 - interactivObject.Texture.Height / 2));
        createForm = new CreateNewObject();
      }

      public override void Update()
      {
        mKSLast = mKsCurrent;
        mKsCurrent = Keyboard.GetState();

        UpdateKeyboardInput();
        UpdateActionWheel();
        UpdateMouseClick();
      }

      public override void Draw()
      {
        EngineSettings.Graphics.GraphicsDevice.SetRenderTarget(mRenderTarget);

        DrawBackground();

        mSpriteBatch.Begin();

        interactivObject.Draw(mSpriteBatch);
        //mSpriteBatch.Draw(mTexture, new Vector2((EngineSettings.VirtualResWidth / 2 - mTexture.Width / 2), (EngineSettings.VirtualResHeight / 2 - mTexture.Height / 2 )), Color.White);

        mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), tmpRectangle, Color.White);

        if(DrawActionRectangle)
          foreach (Rectangle r in interactivObject.ActionRectList)
            mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), r, Color.Yellow * 0.5f);

        if(DrawCollisionRectangle)
          foreach (Rectangle r in interactivObject.CollisionRectList)
            mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), r, Color.Green * 0.5f);

        if(DrawPosition && interactivObject.ActionPosition1 != Vector2.Zero)
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), interactivObject.ActionPosition1, new Rectangle((int)interactivObject.ActionPosition1.X - 2, (int)interactivObject.ActionPosition1.Y - 2, 5, 5), Color.Blue);
        if (DrawPosition && interactivObject.ActionPosition2 != Vector2.Zero)
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), interactivObject.ActionPosition2, new Rectangle((int)interactivObject.ActionPosition2.X - 2, (int)interactivObject.ActionPosition2.Y - 2, 5, 5), Color.Blue);

        if(DrawZ && interactivObject.DrawZ > 0)
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), new Vector2(0, interactivObject.DrawZ), new Rectangle(0, interactivObject.DrawZ, 1024, 1), Color.Red);
        mSpriteBatch.End();

        DrawInfo();

        DrawOnScene();
      }

      #endregion

      #region Methods


      protected void DrawInfo()
      {
        mSpriteBatch.Begin();
        mSpriteBatch.DrawString(font, "Rectangle Art: " + art, new Vector2(5, 10), Color.White);
        mSpriteBatch.DrawString(font, "MousePos: " + MouseHelper.Position, new Vector2(5, 30), Color.White);
        mSpriteBatch.End();
      }

      private void UpdateActionWheel()
      {
        if (MouseHelper.Instance.IsWheelUp)
        {
          if (art == RectangleArt.Delete)
            art = RectangleArt.Action;
          else
            art++;
        }
          
        if (MouseHelper.Instance.IsWheelDown)
        {
          if (art == RectangleArt.Action)
            art = RectangleArt.Delete;
          else
            art--;
        }
      }

      private void UpdateMouseClick()
      {
        switch (art)
        {
          case RectangleArt.Action:
            CreateRectangle();
            if(MouseHelper.Instance.IsReleasedLeft && !IsDrawingRectangle)
            {
              if (tmpRectangle.Width > 0 
                  && tmpRectangle.Height > 0)
                interactivObject.ActionRectList.Add(tmpRectangle);
              mRectangleSelectPos1 = Vector2.Zero;
              mRectangleSelectPos2 = Vector2.Zero;
              tmpRectangle = new Rectangle(0, 0, 0, 0);
            }
            break;
          case RectangleArt.Collision:
            CreateRectangle();
            if(MouseHelper.Instance.IsReleasedLeft && !IsDrawingRectangle)
            {
                if (tmpRectangle.Width > 0
                    && tmpRectangle.Height > 0)
                interactivObject.CollisionRectList.Add(tmpRectangle);
              mRectangleSelectPos1 = Vector2.Zero;
              mRectangleSelectPos2 = Vector2.Zero;
              tmpRectangle = new Rectangle(0, 0, 0, 0);
            }
            break;
          case RectangleArt.StartPos1:
            if (MouseHelper.Instance.IsClickedLeft)
              interactivObject.ActionPosition1 = MouseHelper.Position;
            break;
          case RectangleArt.StartPos2:
            if (MouseHelper.Instance.IsClickedLeft)
              interactivObject.ActionPosition2 = MouseHelper.Position;
            break;
          case RectangleArt.Z:
            if (MouseHelper.Instance.IsClickedLeft)
              interactivObject.DrawZ = (int)MouseHelper.Position.Y;
            break;
          case RectangleArt.Delete:
            if (MouseHelper.Instance.IsClickedLeft)
              Delete();
            break;
        }
      }

      private void UpdateKeyboardInput()
      {
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F1))
            DrawActionRectangle = !DrawActionRectangle;
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F2))
            DrawCollisionRectangle = !DrawCollisionRectangle;
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F3))
            DrawPosition = !DrawPosition;
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F4) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F4))
            DrawZ = !DrawZ;

        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F5))
          CreateNewInteractivObject();
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F6) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F6))
          LoadInteractivObject();
        if (mKSLast.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F7) && mKsCurrent.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.F7))
          SaveInteractivObject();
      }

      private void CreateRectangle()
      {

        if (!IsDrawingRectangle && MouseHelper.Instance.IsPressedLeft)
        {
          IsDrawingRectangle = true;
          mRectangleSelectPos1 = new Vector2(MouseHelper.Position.X, MouseHelper.Position.Y);
        }

        if (IsDrawingRectangle && MouseHelper.Instance.IsPressedLeft)
        {
          mRectangleSelectPos2 = new Vector2(MouseHelper.Position.X, MouseHelper.Position.Y);
          tmpRectangle = new Rectangle((int)mRectangleSelectPos1.X, (int)mRectangleSelectPos1.Y, (int)(mRectangleSelectPos2.X - mRectangleSelectPos1.X), (int)(mRectangleSelectPos2.Y - mRectangleSelectPos1.Y));
        }

        if (IsDrawingRectangle && MouseHelper.Instance.IsReleasedLeft)
        {
          IsDrawingRectangle = false;
        }
      }
      
      private void Delete()
      {
        if (DrawZ)
          if (new Rectangle(0, interactivObject.DrawZ, 1024, 1).Contains(MouseHelper.PositionPoint))
            interactivObject.DrawZ = 0;

        if(DrawPosition)
        {
          if (new Rectangle((int)interactivObject.ActionPosition1.X - 2, (int)interactivObject.ActionPosition1.Y - 2, 5, 5).Contains(MouseHelper.PositionPoint))
            interactivObject.ActionPosition1 = Vector2.Zero;
          if (new Rectangle((int)interactivObject.ActionPosition2.X - 2, (int)interactivObject.ActionPosition2.Y - 2, 5, 5).Contains(MouseHelper.PositionPoint))
            interactivObject.ActionPosition2 = Vector2.Zero;
        }

        if(DrawActionRectangle)
          for (int i = 0; i < interactivObject.ActionRectList.Count; i++)
          {
            if (interactivObject.ActionRectList[i].Contains(MouseHelper.PositionPoint))
            {
              interactivObject.ActionRectList.RemoveAt(i);
              return;
            }
          }

        if(DrawCollisionRectangle)
          for (int i = 0; i < interactivObject.CollisionRectList.Count; i++)
          {
            if (interactivObject.CollisionRectList[i].Contains(MouseHelper.PositionPoint))
            {
              interactivObject.CollisionRectList.RemoveAt(i);
              return;
            }
          }
      }

      public void LoadInteractivObject()
      {
        Stream myStream;
        OpenFileDialog openFileDialog1 = new OpenFileDialog();

        openFileDialog1.InitialDirectory = Environment.CurrentDirectory + @"\Content\iObj\";
        openFileDialog1.Filter = "InteractivObject (*.iObj)|*.iObj";
        openFileDialog1.FilterIndex = 2;
        openFileDialog1.RestoreDirectory = true;

        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
          if ((myStream = openFileDialog1.OpenFile()) != null)
          {
            interactivObject = new InteractiveObject();
            XmlSerializer xml = new XmlSerializer(typeof(InteractiveObject));

            TextReader reader = new StreamReader(myStream);
            interactivObject = (InteractiveObject)xml.Deserialize(myStream);
            interactivObject.Texture = TextureManager.Instance.GetElementByString(interactivObject.TextureName);
            reader.Close();
            myStream.Close();
          }
        }
      }

      public void SaveInteractivObject()
      {
        Stream myStream;
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();

        saveFileDialog1.InitialDirectory = Environment.CurrentDirectory + @"\Content\iObj\";
        saveFileDialog1.Filter = "InteractivObject (*.iObj)|*.iObj";
        saveFileDialog1.FilterIndex = 2;
        saveFileDialog1.RestoreDirectory = true;
		saveFileDialog1.FileName = interactivObject.TextureName + ".iObj";

        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
        {
          if ((myStream = saveFileDialog1.OpenFile()) != null)
          {
				XmlSerializer xml = new XmlSerializer(typeof(InteractiveObject));
				InteractiveObject io = new InteractiveObject();

				io.Texture = interactivObject.Texture;
				io.TextureName = interactivObject.TextureName;

				io.Position = Vector2.Zero;

                Vector2 ScreenCenter = new Vector2(EngineSettings.VirtualResWidth / 2, EngineSettings.VirtualResHeight / 2);// -new Vector2(io.Texture.Width / 2, io.Texture.Height / 2);

			  for(int i = 0; i < interactivObject.ActionRectList.Count ;i++)
			  {
				  io.ActionRectList.Add(new Rectangle
                      (interactivObject.ActionRectList[i].X - (int)ScreenCenter.X,
                       interactivObject.ActionRectList[i].Y - (int)ScreenCenter.Y,
					   interactivObject.ActionRectList[i].Width,
					   interactivObject.ActionRectList[i].Height));
			  }

			  for (int i = 0; i < interactivObject.CollisionRectList.Count; i++)
			  {
				  io.CollisionRectList.Add(new Rectangle
                      (interactivObject.CollisionRectList[i].X - (int)ScreenCenter.X,
                       interactivObject.CollisionRectList[i].Y - (int)ScreenCenter.Y,
					   interactivObject.CollisionRectList[i].Width,
					   interactivObject.CollisionRectList[i].Height));
			  }

			  if (interactivObject.ActionPosition1 != Vector2.Zero)
				  io.ActionPosition1 = interactivObject.ActionPosition1 - ScreenCenter;
			  if (interactivObject.ActionPosition2 != Vector2.Zero)
				  io.ActionPosition2 = interactivObject.ActionPosition2 - ScreenCenter;

			  if (interactivObject.DrawZ != 0)
				  io.DrawZ = interactivObject.DrawZ - (int)ScreenCenter.Y;

				TextWriter writer = new StreamWriter(myStream);
				xml.Serialize(writer, io);
				writer.Close();
				myStream.Close();
          }
        }
      }

      public void CreateNewInteractivObject()
      {
        createForm.ShowDialog();

        interactivObject.ActionId = 0;
        interactivObject.ActionPosition1 = Vector2.Zero;
        interactivObject.ActionPosition2 = Vector2.Zero;
        interactivObject.ActionRectList.Clear();
        interactivObject.CollisionRectList.Clear();
        interactivObject.DrawZ = 0;
        interactivObject.TextureName = NewTextureName;
        interactivObject.Texture = TextureManager.Instance.GetElementByString(interactivObject.TextureName);
		interactivObject.Position = new Vector2(EngineSettings.VirtualResWidth / 2 - interactivObject.Texture.Width/2, EngineSettings.VirtualResHeight / 2 - interactivObject.Texture.Height/2);
      }
      #endregion
    }
  }
