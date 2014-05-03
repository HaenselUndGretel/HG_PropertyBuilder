using KryptonEngine;
using KryptonEngine.Controls;
using KryptonEngine.Manager;
using KryptonEngine.SceneManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyBuilder.GameContent.Scenes
{
  enum RectangleArt 
  {
    Action,
    Collision,
    StartPos1,
    StartPos2,
    Z,
    Delete
  }
  class PropertyScene : Scene
  {
      #region Properties

    protected int mDrawZ = 0;
    protected int mActionId = -1;

    protected List<Rectangle> mActionRectangle = new List<Rectangle>();
    protected List<Rectangle> mCollisionRectangle = new List<Rectangle>();

    protected Vector2 mActionStartPos1 = Vector2.Zero;
    protected Vector2 mActionStartPos2 = Vector2.Zero;

    protected bool DrawActionRectangle = true;
    protected bool DrawCollisionRectangle = true;
    protected bool DrawPosition = true;
    protected bool DrawZ = true;
    protected bool IsDrawingRectangle = false;

    protected SpriteFont font;

    protected Vector2 mRectangleSelectPos1 = Vector2.Zero;
    protected Vector2 mRectangleSelectPos2 = Vector2.Zero;

    protected RectangleArt art = RectangleArt.Action;

    protected Vector2 mRectanglePos1 = Vector2.Zero;
    protected Vector2 mRectanglePos2 = Vector2.Zero;

    protected Texture2D mTexture;
    protected Rectangle tmpRectangle;

    protected bool isInCollision = false;
    protected bool isInAction = false;

      #endregion

      #region Getter & Setter

      #endregion

      #region Constructor

      public PropertyScene(String pSceneName)
        : base(pSceneName)
      {
      }
      #endregion

      #region Override Methods

      public override void Initialize()
      {
        mClearColor = Color.Black;
      }

      public override void LoadContent()
      {
        font = FontManager.Instance.Add("font", @"font\font");
        mTexture = TextureManager.Instance.Add("Hansel", @"gfx\hansel_cutout");
      }

      public override void Update()
      {
        UpdateActionWheel();
        UpdateMouseClick();
      }

      public override void Draw()
      {
        EngineSettings.Graphics.GraphicsDevice.SetRenderTarget(mRenderTarget);

        DrawBackground();

        mSpriteBatch.Begin();

        mSpriteBatch.Draw(mTexture, new Vector2((EngineSettings.VirtualResWidth / 2 - mTexture.Width / 2), (EngineSettings.VirtualResHeight / 2 - mTexture.Height / 2 )), Color.White);

        mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), tmpRectangle, Color.White);

        if(DrawActionRectangle)
          foreach (Rectangle r in mActionRectangle)
            mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), r, Color.Yellow * 0.5f);

        if(DrawCollisionRectangle)
          foreach (Rectangle r in mCollisionRectangle)
            mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), r, Color.Green * 0.5f);

        if(DrawPosition)
        {
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), mActionStartPos1, new Rectangle((int)mActionStartPos1.X - 2, (int)mActionStartPos1.Y - 2, 5, 5), Color.Blue);
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), mActionStartPos2, new Rectangle((int)mActionStartPos2.X - 2, (int)mActionStartPos2.Y - 2, 5, 5), Color.Blue);
        }

        if(DrawZ)
          mSpriteBatch.Draw(TextureManager.Instance.GetElementByString("pixel"), new Vector2(0,mDrawZ),new Rectangle(0, mDrawZ, 1024,1), Color.Red);
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
                mActionRectangle.Add(tmpRectangle);
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
                mCollisionRectangle.Add(tmpRectangle);
              mRectangleSelectPos1 = Vector2.Zero;
              mRectangleSelectPos2 = Vector2.Zero;
              tmpRectangle = new Rectangle(0, 0, 0, 0);
            }
            break;
          case RectangleArt.StartPos1:
            if (MouseHelper.Instance.IsClickedLeft)
              mActionStartPos1 = MouseHelper.Position;
            break;
          case RectangleArt.StartPos2:
            if (MouseHelper.Instance.IsClickedLeft)
              mActionStartPos2 = MouseHelper.Position;
            break;
          case RectangleArt.Z:
            if (MouseHelper.Instance.IsClickedLeft)
              mDrawZ = (int)MouseHelper.Position.Y;
            break;
          case RectangleArt.Delete:
            if (MouseHelper.Instance.IsClickedLeft)
              DeleteRectangle();
            break;
        }
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
      
      private void DeleteRectangle()
      {
        bool somethingDeleted = false;
        for(int i = 0; i < mActionRectangle.Count; i++)
        {
          if(mActionRectangle[i].Contains(MouseHelper.PositionPoint))
          {
            somethingDeleted = true;
            mActionRectangle.RemoveAt(i);
            return;
          }
        }

        for (int i = 0; i < mCollisionRectangle.Count; i++)
        {
          if (mCollisionRectangle[i].Contains(MouseHelper.PositionPoint))
          {
            somethingDeleted = true;
            mCollisionRectangle.RemoveAt(i);
            return;
          }
        }

      }
      #endregion
    }
  }
