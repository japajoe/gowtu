using OpenTK.Graphics.OpenGL;

namespace Gowtu
{
    public sealed class FrameBufferObject : BufferObject
    {
        private int textureAttachmentId;
        private int depthAttachmentId;
        private int width;
        private int height;

        public int TextureAttachmentId
        {
            get => textureAttachmentId;
        }

        public int DepthAttachmentId
        {
            get => depthAttachmentId;
        }

        public FrameBufferObject(int width, int height) : base()
        {
            textureAttachmentId = 0;
            depthAttachmentId = 0;
            this.width = width;
            this.height = height;
        }

        public override void Generate()
        {
            if(width == 0 || height == 0)
                throw new System.Exception("FrameBufferObject width and height must be greater than 0");

            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            
            textureAttachmentId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, textureAttachmentId);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, System.IntPtr.Zero);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureAttachmentId, 0);
            
            depthAttachmentId = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthAttachmentId);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthAttachmentId);

            FramebufferStatus status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if(status != FramebufferStatus.FramebufferComplete)
                throw new System.Exception("FrameBufferObject is not complete. Status: " + status);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2d, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }

        public override void Delete()
        {
            if(id > 0)
            {
                GL.DeleteFramebuffer(id);
                id = 0;
            }

            if(textureAttachmentId > 0)
            {
                GL.DeleteTexture(textureAttachmentId);
                textureAttachmentId = 0;
            }

            if(depthAttachmentId > 0)
            {
                GL.DeleteRenderbuffer(depthAttachmentId);
                depthAttachmentId = 0;
            }
        }

        public override void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }

        public override void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Resize(int width, int height)
        {
            if (id == 0)
                throw new System.Exception("FrameBufferObject has not been generated");

            if (width == 0 || height == 0)
                throw new System.Exception("Width and height must be greater than 0");

            this.width = width;
            this.height = height;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            GL.BindTexture(TextureTarget.Texture2d, textureAttachmentId);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, System.IntPtr.Zero);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, textureAttachmentId, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthAttachmentId);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthAttachmentId);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2d, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
    }
}