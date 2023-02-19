using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Texture
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// The Base class of BTI and TPL
    /// </summary>
    public abstract partial class JUTTexture : List<JUTTexture.TexEntry>, IDisposable
    {

        private bool disposedValue;

        /// <summary>
        /// The full path of this file.
        /// </summary>
        public string FileName { get; set; } = null;

        public JUTTexture() { }

        public JUTTexture(Stream stream) => Read(stream);

        public JUTTexture(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open);
            Read(fs);
            fs.Close();
            FileName = filepath;
        }

        public virtual void Save(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Create);
            Write(fs);
            fs.Close();
            FileName = filepath;
        }
        public virtual void Save(Stream stream) => Write(stream);
        public virtual void Open(Stream stream) => Read(stream);

        protected abstract void Read(Stream stream);
        protected abstract void Write(Stream stream);

        public bool ImageEquals(JUTTexture entry) => ListEx.Equals(this, entry);

        public override bool Equals(object obj) => obj is JUTTexture tex && tex.FileName.Equals(FileName) && ImageEquals(tex);

        public static bool operator ==(JUTTexture texture1, JUTTexture texture2) => texture1.Equals(texture2);

        public static bool operator !=(JUTTexture texture1, JUTTexture texture2) => !(texture1 == texture2);

        public override int GetHashCode() => 901043656 + FileName.GetHashCode() + ListEx.GetHashCode(this);

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in this)
                        item.Dispose();
                }
                disposedValue = true;
            }
        }
        ~JUTTexture()
        {
            Dispose(disposing: true);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
