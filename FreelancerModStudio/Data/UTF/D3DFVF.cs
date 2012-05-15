using System;

namespace FreelancerModStudio.Data.UTF
{
    [Flags]
    public enum D3DFVF : ushort
    {
        //Reserved0 = 0x0001,

        /// <summary>
        /// Vertex format includes the position of an untransformed vertex. This flag cannot be used with the D3DFVF_XYZRHW flag.
        /// </summary>
        XYZ = 0x0002,

        //XYZRHW = 0x0004,
        //XYZB1 = 0x0006,
        //XYZB2 = 0x0008,
        //XYZB3 = 0x000a,
        //XYZB4 = 0x000c,
        //XYZB5 = 0x000e,

        /// <summary>
        /// Vertex format includes a vertex normal vector. This flag cannot be used with the D3DFVF_XYZRHW flag.
        /// </summary>
        Normal = 0x0010,

        //Reserved1 = 0x0020,

        /// <summary>
        /// Vertex format includes a diffuse color component.
        /// </summary>
        Diffuse = 0x0040,

        //Specular = 0x0080,

        //TexcountMask = 0x0f00,
        //Tex0 = 0x0000,

        /// <summary>
        /// Number of texture coordinate sets for this vertex. The actual values for these flags are not sequential.
        /// </summary>
        Tex1 = 0x0100,

        /// <summary>
        /// Number of texture coordinate sets for this vertex. The actual values for these flags are not sequential.
        /// </summary>
        Tex2 = 0x0200,

        //Tex3 = 0x0300,

        /// <summary>
        /// Number of texture coordinate sets for this vertex. The actual values for these flags are not sequential.
        /// </summary>
        Tex4 = 0x0400,

        Tex5 = 0x0500,
        //Tex6 = 0x0600,
        //Tex7 = 0x0700,
        //Tex8 = 0x0800
    }
}
