using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hack.io
{
    public enum ShaderAttributeIds
    {
        PosMtxIndex = 0,
        Position = 1,
        Normal = 2,
        Binormal = 3,
        Color0 = 4,
        Color1 = 5,
        Tex0 = 6,
        Tex1 = 7,
        Tex2 = 8,
        Tex3 = 9,
        Tex4 = 10,
        Tex5 = 11,
        Tex6 = 12,
        Tex7 = 13,
    }

    public static class ShaderGenerator
    {
        public static (string Vert, string Frag) GenerateShader(BMD.MAT3.Material Material, BMD.SHP1.Shape shape)
        {
            CultureInfo forceusa = new CultureInfo("en-US");
            StringBuilder Vert = new StringBuilder(), Frag = new StringBuilder();

            #region Vertex Shader
            Vert.AppendLine("#version 330");
            //TODO: J3DView has 2 lines I can't figure out what to do with

            string position = "";
            switch (shape.MatrixType)
            {
                case BMD.DisplayFlags.SingleMatrix:
                    Vert.AppendLine("uniform int matrix_index;");
                    Vert.AppendLine("uniform samplerBuffer matrix_table;");
                    Vert.AppendLine("#define MATRIX_ROW(i) texelFetch(matrix_table,3*matrix_index + i)");
                    position = "view_matrix*vec4(dot(MATRIX_ROW(0),position),dot(MATRIX_ROW(1),position),dot(MATRIX_ROW(2),position),1.0)";
                    break;
                case BMD.DisplayFlags.Billboard:
                    position = "(position.xyz + view_matrix[3])";
                    break;
                case BMD.DisplayFlags.BillboardY:
                    throw new Exception("No Y Billboard support!");
                case BMD.DisplayFlags.MultiMatrix:
                    Vert.AppendLine($"layout(location={(int)BMD.GXVertexAttribute.PositionMatrixIdx}) in int matrix_index;");
                    Vert.AppendLine("uniform samplerBuffer matrix_table;");
                    Vert.AppendLine("#define MATRIX_ROW(i) texelFetch(matrix_table,3*matrix_index + i)");
                    position = "view_matrix*vec4(dot(MATRIX_ROW(0),position),dot(MATRIX_ROW(1),position),dot(MATRIX_ROW(2),position),1.0)";
                    break;
                default:
                    throw new Exception();
            }

            if (shape.Descriptor.CheckAttribute(BMD.GXVertexAttribute.Position))
                Vert.AppendLine($"layout(location={(int)BMD.GXVertexAttribute.Position}) in vec4 position;");

            if (shape.Descriptor.CheckAttribute(BMD.GXVertexAttribute.Normal))
                Vert.AppendLine($"layout(location={(int)BMD.GXVertexAttribute.Position}) in vec3 normal;");


            if (shape.Descriptor.CheckAttribute(BMD.GXVertexAttribute.Color0))
                Vert.AppendLine($"layout(location={(int)BMD.GXVertexAttribute.Position}) in vec4 color0;");
            if (shape.Descriptor.CheckAttribute(BMD.GXVertexAttribute.Color1))
                Vert.AppendLine($"layout(location={(int)BMD.GXVertexAttribute.Position}) in vec4 color1;");

            for (int i = 0; i < 8; i++)
            {
                if (!shape.Descriptor.CheckAttribute(BMD.GXVertexAttribute.Tex0 + i))
                    continue;
                Vert.AppendLine($"layout(location={(int)(BMD.GXVertexAttribute.Tex0 + i)}) in vec2 texcoord{i};");
            }

            Vert.AppendLine();
            Vert.AppendLine("\nvoid main()\n{");
            Vert.AppendLine($"gl_Position = projection_matrix*vec4({position},1.0);");
            #endregion

            #region Fragment Shader
            Frag.AppendLine("#version 120");
            Frag.AppendLine();
            for (int i = 0; i < 8; i++)
            {
                if (Material.Textures[i] is null)
                    continue;
                Frag.AppendLine("uniform sampler2D texture" + i.ToString() + ";");
            }
            Frag.AppendLine("float truncc1(float c)");
            Frag.AppendLine("{");
            Frag.AppendLine("    return (c == 0.0) ? 0.0 : ((fract(c) == 0.0) ? 1.0 : fract(c));");
            Frag.AppendLine("}");
            Frag.AppendLine();
            Frag.AppendLine("vec3 truncc3(vec3 c)");
            Frag.AppendLine("{");
            Frag.AppendLine("    return vec3(truncc1(c.r), truncc1(c.g), truncc1(c.b));");
            Frag.AppendLine("}");
            Frag.AppendLine();
            Frag.AppendLine("void main()");
            Frag.AppendLine("{");

            for (int i = 0; i < 4; i++)
            {
                int _i = (i == 0) ? 3 : i - 1; // ???
                if (Material.TevColors[_i].HasValue)
                {
                    Frag.AppendFormat(forceusa, "    vec4 {0} = vec4({1}, {2}, {3}, {4});\n", outputregs[i], Material.TevColors[_i].Value.R, Material.TevColors[_i].Value.G, Material.TevColors[_i].Value.B, Material.TevColors[_i].Value.A);
                }
                else
                {
                    Frag.AppendFormat(forceusa, "    vec4 {0} = vec4({1}, {2}, {3}, {4});\n", outputregs[i], 1.0f, 0.0f, 1.0f, 1.0f);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                Frag.AppendFormat(forceusa, "    vec4 k{0} = vec4({1}, {2}, {3}, {4});\n", i, Material.KonstColors[i].Value.R, Material.KonstColors[i].Value.G, Material.KonstColors[i].Value.B, Material.KonstColors[i].Value.A);
            }
            Frag.AppendLine("    vec4 texcolor, rascolor, konst;");

            for (int i = 0; i < Material.NumTevStagesCount; i++)
            {
                Frag.AppendLine("\n    // TEV Stage " + i.ToString());

                // TEV inputs
                // for registers prev/0/1/2: use fract() to emulate truncation
                // if they're selected into a, b or c
                string rout, a, b, c, d, operation = "";

                if ((int)Material.ColorSels[i] != 255)
                    Frag.AppendLine("    konst.rgb = " + c_konstsel[(int)Material.ColorSels[i]] + ";");
                else
                    Frag.AppendLine("    konst.rgb = " + c_konstsel[0] + ";");
                if ((int)Material.AlphaSels[i] != 255)
                    Frag.AppendLine("    konst.a = " + a_konstsel[(int)Material.AlphaSels[i]] + ";");
                if (Material.TevOrders[i].Value.TexMap != BMD.MAT3.TexMapId.Null && Material.TevOrders[i].Value.TexCoord != BMD.MAT3.TexCoordId.Null)
                    Frag.AppendFormat("    texcolor = texture2D(texture{0}, gl_TexCoord[{1}].st);\n", (int)Material.TevOrders[i].Value.TexMap, (int)Material.TevOrders[i].Value.TexCoord);
                Frag.AppendLine("    rascolor = gl_Color;");
                // TODO: take mat.TevOrder[i].ChanId into account
                // TODO: tex/ras swizzle? (important or not?)
                //mat.TevSwapMode[0].

                if (Material.TevOrders[i].Value.ChannelId != BMD.MAT3.Material.TevOrder.GXColorChannelId.Color0A0)
                {
                    //throw new Exception("PLEASE investigate how to support this!");
                }

                //TEV Stage Colour
                rout = outputregs[(int)Material.TevStages[i].Value.ColorRegId] + ".rgb";
                a = c_inputregs[(int)Material.TevStages[i].Value.ColorInA];
                b = c_inputregs[(int)Material.TevStages[i].Value.ColorInB];
                c = c_inputregs[(int)Material.TevStages[i].Value.ColorInC];
                d = c_inputregsD[(int)Material.TevStages[i].Value.ColorInD];

                switch (Material.TevStages[i].Value.ColorOp)
                {
                    case BMD.MAT3.Material.TevStage.TevOp.Add:
                        operation = "    {0} = ({4} + mix({1},{2},{3}) + vec3({5},{5},{5})) * vec3({6},{6},{6});";
                        if (Material.TevStages[i].Value.ColorClamp)
                            operation += "\n    {0} = clamp({0}, vec3(0.0,0.0,0.0), vec3(1.0,1.0,1.0));";
                        break;

                    case BMD.MAT3.Material.TevStage.TevOp.Sub:
                        operation = "    {0} = ({4} - mix({1},{2},{3}) + vec3({5},{5},{5})) * vec3({6},{6},{6});";
                        if (Material.TevStages[i].Value.ColorClamp)
                            operation += "\n    {0} = clamp({0}, vec3(0.0,0.0,0.0), vec3(1.0,1.0,1.0));";
                        break;

                    case BMD.MAT3.Material.TevStage.TevOp.Comp_R8_GT:
                        operation = "    {0} = {4} + ((({1}).r > ({2}).r) ? {3} : vec(0.0,0.0,0.0));";
                        break;

                    default:
                        operation = "    {0} = vec3(1.0,0.0,1.0);";
                        throw new Exception("!colorop " + Material.TevStages[i].Value.ColorOp.ToString());
                }
                operation = string.Format(operation, rout, a, b, c, d, tevbias[(int)Material.TevStages[i].Value.ColorBias], tevscale[(int)Material.TevStages[i].Value.ColorScale]);
                Frag.AppendLine(operation);

                //TEV Stage Alpha
                rout = outputregs[(int)Material.TevStages[i].Value.AlphaRegId] + ".a";
                a = a_inputregs[(int)Material.TevStages[i].Value.AlphaInA];
                b = a_inputregs[(int)Material.TevStages[i].Value.AlphaInB];
                c = a_inputregs[(int)Material.TevStages[i].Value.AlphaInC];
                d = a_inputregsD[(int)Material.TevStages[i].Value.AlphaInD];

                switch (Material.TevStages[i].Value.AlphaOp)
                {
                    case BMD.MAT3.Material.TevStage.TevOp.Add:
                        operation = "    {0} = ({4} + mix({1},{2},{3}) + {5}) * {6};";
                        if (Material.TevStages[i].Value.AlphaClamp)
                            operation += "\n   {0} = clamp({0}, 0.0, 1.0);";
                        break;

                    case BMD.MAT3.Material.TevStage.TevOp.Sub:
                        operation = "    {0} = ({4} - mix({1},{2},{3}) + {5}) * {6};";
                        if (Material.TevStages[i].Value.AlphaClamp)
                            operation += "\n   {0} = clamp({0}, 0.0, 1.0);";
                        break;

                    default:
                        operation = "    {0} = 1.0;";
                        throw new Exception("!alphaop " + Material.TevStages[i].Value.AlphaOp.ToString());
                }

                operation = string.Format(operation, rout, a, b, c, d, tevbias[(int)Material.TevStages[i].Value.AlphaBias], tevscale[(int)Material.TevStages[i].Value.AlphaScale]);
                Frag.AppendLine(operation);
            }

            Frag.AppendLine("");
            Frag.AppendLine("   gl_FragColor.rgb = truncc3(rprev.rgb);");
            Frag.AppendLine("   gl_FragColor.a = truncc1(rprev.a);");
            Frag.AppendLine("");

            Frag.AppendLine("    // Alpha test");
            if (Material.AlphCompare.Operation == BMD.MAT3.Material.AlphaCompare.AlphaOp.Or && (Material.AlphCompare.Comp0 == BMD.MAT3.Material.AlphaCompare.CompareType.Always || Material.AlphCompare.Comp1 == BMD.MAT3.Material.AlphaCompare.CompareType.Always))
            {
                // always pass -- do nothing :)
                Frag.AppendLine("    // Alpha test will ALWAYS PASS");
            }
            else if (Material.AlphCompare.Operation == BMD.MAT3.Material.AlphaCompare.AlphaOp.And && (Material.AlphCompare.Comp0 == BMD.MAT3.Material.AlphaCompare.CompareType.Never || Material.AlphCompare.Comp1 == BMD.MAT3.Material.AlphaCompare.CompareType.Never))
            {
                // never pass
                // (we did all those color/alpha calculations for uh, nothing ;_; )
                Frag.AppendLine("    discard; //Alpha test will NEVER PASS");
            }
            else
            {
                string compare0 = string.Format(forceusa, alphacompare[(int)Material.AlphCompare.Comp0], "gl_FragColor.a", (float)Material.AlphCompare.Reference0 / 255f);
                string compare1 = string.Format(forceusa, alphacompare[(int)Material.AlphCompare.Comp1], "gl_FragColor.a", (float)Material.AlphCompare.Reference1 / 255f);
                string fullcompare = "";

                if (Material.AlphCompare.Operation == BMD.MAT3.Material.AlphaCompare.AlphaOp.Or)
                {
                    if (Material.AlphCompare.Comp0 == BMD.MAT3.Material.AlphaCompare.CompareType.Never)
                        fullcompare = compare1;
                    else if (Material.AlphCompare.Comp1 == BMD.MAT3.Material.AlphaCompare.CompareType.Never)
                        fullcompare = compare0;
                }
                else if (Material.AlphCompare.Operation == BMD.MAT3.Material.AlphaCompare.AlphaOp.And)
                {
                    if (Material.AlphCompare.Comp0 == BMD.MAT3.Material.AlphaCompare.CompareType.Always)
                        fullcompare = compare1;
                    else if (Material.AlphCompare.Comp1 == BMD.MAT3.Material.AlphaCompare.CompareType.Always)
                        fullcompare = compare0;
                }

                if (fullcompare == "")
                    fullcompare = string.Format(alphacombine[(int)Material.AlphCompare.Operation], compare0, compare1);

                Frag.AppendLine("    if (!(" + fullcompare + ")) discard;");
            }

            Frag.AppendLine("}");
            #endregion

            return (Vert.ToString(), Frag.ToString());
        }

        static string[] texgensrc = { "normalize(gl_Vertex)", "vec4(gl_Normal,1.0)", "argh", "argh",
                                     "gl_MultiTexCoord0", "gl_MultiTexCoord1", "gl_MultiTexCoord2", "gl_MultiTexCoord3",
                                     "gl_MultiTexCoord4", "gl_MultiTexCoord5", "gl_MultiTexCoord6", "gl_MultiTexCoord7" };

        static string[] outputregs = { "rprev", "r0", "r1", "r2" };

        static string[] c_inputregs = { "truncc3(rprev.rgb)", "truncc3(rprev.aaa)", "truncc3(r0.rgb)", "truncc3(r0.aaa)",
                                        "truncc3(r1.rgb)", "truncc3(r1.aaa)", "truncc3(r2.rgb)", "truncc3(r2.aaa)",
                                       "texcolor.rgb", "texcolor.aaa", "rascolor.rgb", "rascolor.aaa",
                                       "vec3(1.0,1.0,1.0)", "vec3(0.5,0.5,0.5)", "konst.rgb", "vec3(0.0,0.0,0.0)" };
        static string[] c_inputregsD = { "rprev.rgb", "rprev.aaa", "r0.rgb", "r0.aaa",
                                        "r1.rgb", "r1.aaa", "r2.rgb", "r2.aaa",
                                       "texcolor.rgb", "texcolor.aaa", "rascolor.rgb", "rascolor.aaa",
                                       "vec3(1.0,1.0,1.0)", "vec3(0.5,0.5,0.5)", "konst.rgb", "vec3(0.0,0.0,0.0)" };
        static string[] c_konstsel = { "vec3(1.0,1.0,1.0)", "vec3(0.875,0.875,0.875)", "vec3(0.75,0.75,0.75)", "vec3(0.625,0.625,0.625)",
                                      "vec3(0.5,0.5,0.5)", "vec3(0.375,0.375,0.375)", "vec3(0.25,0.25,0.25)", "vec3(0.125,0.125,0.125)",
                                      "", "", "", "", "k0.rgb", "k1.rgb", "k2.rgb", "k3.rgb",
                                      "k0.rrr", "k1.rrr", "k2.rrr", "k3.rrr", "k0.ggg", "k1.ggg", "k2.ggg", "k3.ggg",
                                      "k0.bbb", "k1.bbb", "k2.bbb", "k3.bbb", "k0.aaa", "k1.aaa", "k2.aaa", "k3.aaa" };

        static string[] a_inputregs = { "truncc1(rprev.a)", "truncc1(r0.a)", "truncc1(r1.a)", "truncc1(r2.a)",
                                       "texcolor.a", "rascolor.a", "konst.a", "0.0" };
        static string[] a_inputregsD = { "rprev.a", "r0.a", "r1.a", "r2.a",
                                       "texcolor.a", "rascolor.a", "konst.a", "0.0" };
        static string[] a_konstsel = { "1.0", "0.875", "0.75", "0.625", "0.5", "0.375", "0.25", "0.125",
                                      "", "", "", "", "", "", "", "",
                                      "k0.r", "k1.r", "k2.r", "k3.r", "k0.g", "k1.g", "k2.g", "k3.g",
                                      "k0.b", "k1.b", "k2.b", "k3.b", "k0.a", "k1.a", "k2.a", "k3.a" };

        static string[] tevbias = { "0.0", "0.5", "-0.5","0.0" };
        static string[] tevscale = { "1.0", "2.0", "4.0", "0.5" };

        static string[] alphacompare = { "{0} != {0}", "{0} < {1}", "{0} == {1}", "{0} <= {1}", "{0} > {1}", "{0} != {1}", "{0} >= {1}", "{0} == {0}" };
        //static  string[] alphacombine = { "all(bvec2({0},{1}))", "any(bvec2({0},{1}))", "any(bvec2(all(bvec2({0},!{1})),all(bvec2(!{0},{1}))))", "any(bvec2(all(bvec2({0},{1})),all(bvec2(!{0},!{1}))))" };
        static string[] alphacombine = { "({0}) && ({1})", "({0}) || ({1})", "(({0}) && (!({1}))) || ((!({0})) && ({1}))", "(({0}) && ({1})) || ((!({0})) && (!({1})))" };

        // yes, oldstyle shaders
        // I would use version 130 or above but there are certain
        // of their new designs I don't agree with. Namely, what's
        // up with removing texture coordinates. That's just plain
        // retarded.
    }
}
