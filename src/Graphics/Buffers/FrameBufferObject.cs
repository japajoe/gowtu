using System;
using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class FrameBufferObject : BufferObject
    {
        private int textureAttachment;
        private int depthAttachment;
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ColorTexture => textureAttachment;
        public int DepthTexture => depthAttachment;

        public FrameBufferObject(int width, int height) : base()
        {
            Width = width;
            Height = height;
        }

        public override void Generate()
        {
            // Create FBO
            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            // Create color texture
            textureAttachment = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, textureAttachment);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            SetDefaultTextureParameters();
            
            // Attach color texture
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureAttachment, 0);

            // Create depth texture
            depthAttachment = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, depthAttachment);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent24, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            SetDepthTextureParameters();
            
            // Attach depth texture
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2d, depthAttachment, 0);

            CheckFramebufferStatus();
            Unbind();
        }

        private void SetDefaultTextureParameters()
        {
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        private void SetDepthTextureParameters()
        {
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            // Resize color texture
            GL.BindTexture(TextureTarget.Texture2d, textureAttachment);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // Resize depth texture
            GL.BindTexture(TextureTarget.Texture2d, depthAttachment);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent24, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2d, 0);
        }

        public override void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.Viewport(0, 0, Width, Height);
        }

        public override void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void CheckFramebufferStatus()
        {
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferStatus.FramebufferComplete)
            {
                throw new Exception($"Framebuffer incomplete: {status}");
            }
        }

        public override void Delete()
        {
            if(id > 0)
            {
                GL.DeleteFramebuffer(id);
                id = 0;
            }
            if(textureAttachment > 0)
            {
                GL.DeleteTexture(textureAttachment);
                textureAttachment = 0;
            }
            if(depthAttachment > 0)
            {
                GL.DeleteTexture(depthAttachment);
                depthAttachment = 0;
            }
        }
    }

    // public sealed class FrameBufferObject : BufferObject
    // {
    //     private int textureAttachmentId;
    //     private int depthAttachmentId;
    //     private int width;
    //     private int height;

    //     public int TextureAttachmentId
    //     {
    //         get => textureAttachmentId;
    //     }

    //     public int DepthAttachmentId
    //     {
    //         get => depthAttachmentId;
    //     }

    //     public FrameBufferObject(int width, int height) : base()
    //     {
    //         textureAttachmentId = 0;
    //         depthAttachmentId = 0;
    //         this.width = width;
    //         this.height = height;
    //     }

    //     public override void Generate()
    //     {
    //         if(width == 0 || height == 0)
    //             throw new System.Exception("FrameBufferObject width and height must be greater than 0");

    //         id = GL.GenFramebuffer();
    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            
    //         textureAttachmentId = GL.GenTexture();
    //         GL.BindTexture(TextureTarget.Texture2d, textureAttachmentId);
    //         GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, System.IntPtr.Zero);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    //         GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureAttachmentId, 0);
            
    //         depthAttachmentId = GL.GenRenderbuffer();
    //         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthAttachmentId);
    //         GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
    //         GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthAttachmentId);

    //         FramebufferStatus status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

    //         if(status != FramebufferStatus.FramebufferComplete)
    //             throw new System.Exception("FrameBufferObject is not complete. Status: " + status);

    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    //         GL.BindTexture(TextureTarget.Texture2d, 0);
    //         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    //     }

    //     public override void Delete()
    //     {
    //         if(id > 0)
    //         {
    //             GL.DeleteFramebuffer(id);
    //             id = 0;
    //         }

    //         if(textureAttachmentId > 0)
    //         {
    //             GL.DeleteTexture(textureAttachmentId);
    //             textureAttachmentId = 0;
    //         }

    //         if(depthAttachmentId > 0)
    //         {
    //             GL.DeleteRenderbuffer(depthAttachmentId);
    //             depthAttachmentId = 0;
    //         }
    //     }

    //     public override void Bind()
    //     {
    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
    //     }

    //     public override void Unbind()
    //     {
    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    //     }

    //     public void Resize(int width, int height)
    //     {
    //         if (id == 0)
    //             throw new System.Exception("FrameBufferObject has not been generated");

    //         if (width == 0 || height == 0)
    //             throw new System.Exception("Width and height must be greater than 0");

    //         this.width = width;
    //         this.height = height;

    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

    //         GL.BindTexture(TextureTarget.Texture2d, textureAttachmentId);
    //         GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, System.IntPtr.Zero);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    //         GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    //         GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureAttachmentId, 0);

    //         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthAttachmentId);
    //         GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
    //         GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthAttachmentId);

    //         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    //         GL.BindTexture(TextureTarget.Texture2d, 0);
    //         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    //     }
    // }
}