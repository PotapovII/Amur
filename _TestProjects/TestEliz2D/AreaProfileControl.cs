using MeshLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeometryLib;
using System.Globalization;
using System.IO;
using GeometryLib.Areas;

namespace TestEliz2D
{
    public partial class AreaProfileControl : UserControl
    {
        AbsBedFormParam BedParam;
        //
        Parameter[] GenParam = new Parameter[3];
        //
        ManagerOfChildsAreaBed mChildsBed = new ManagerOfChildsAreaBed();
        ManagerOfChildsGenerators mChildsGen = new ManagerOfChildsGenerators();
        double P = 1, Q = 1;
        public int NyMiddle = 20, NyTop = 0, NyBottom = 0, NyAll = 0;
        public double dyTop = 0, dyBottom = 0, bottomLayer = 0, topLayer = 0;
        double L1, L2, L3;
        public double H=1;
        int Nx;
        int LayerIndex = 1;
        int[] SelectedIndexesLayers = new int[3];

        public AreaProfileControl()
        {
            InitializeComponent();
            //
            InitValues();
            //
            string[] names = mChildsBed.GetChildsNames();
            lstbx_BedType.Items.AddRange(names);
            //
            names = mChildsGen.GetChildsNames();
            lstbx_GenType.Items.AddRange(names);

            try
            {
                lstbx_BedType.SelectedIndex = 2;
                BedParam = mChildsBed.CreateDetermParam(2);
                prpGridBed.SelectedObject = BedParam;
                //
                Lbl_Lall.Text = CalcLAll().ToString();
                //
                lstbx_GenType.SelectedIndex = 0;
                GenParam[1] = mChildsGen.CreateDetermParam(0);
                prpGridGen.SelectedObject = GenParam[1];

                //
                rdbBottomLayer.Visible = false;
                rdbTopLayer.Visible = false;
            }
            catch { }
        }
        //
        void InitValues()
        {
            H = 1;
            L1 = 1;
            L2 = 2;
            L3 = 2;
            topLayer = 0;
            bottomLayer = 0;
            NyMiddle = 20;
            Nx = 100;
            //
            txtbx_H.Text = H.ToString();
            txtbx_L1.Text = L1.ToString();
            txtbx_L2.Text = L2.ToString();
            txtbx_L3.Text = L3.ToString();
            txtbx_TopLayer.Text = topLayer.ToString();
            txtbx_BottomLayer.Text = bottomLayer.ToString(); ;
            txtbx_Ny.Text = NyMiddle.ToString();
            txtbx_Nx.Text = Nx.ToString();
            //
            Lbl_Lall.Text = CalcLAll().ToString();
        }
        private void lstbx_GenType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstbx_GenType.SelectedIndex != SelectedIndexesLayers[LayerIndex])
            {
                GenParam[LayerIndex] = mChildsGen.CreateDetermParam(lstbx_GenType.SelectedIndex);
                SelectedIndexesLayers[LayerIndex] = lstbx_GenType.SelectedIndex;
            }
                prpGridGen.SelectedObject = GenParam[LayerIndex];
            CorrectNx();
            RecalcAllNyDy();

        }

        private void rdbMainArea_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMainArea.Checked)
                LayerIndex = 1;
            LoadParamsGenInPrpGrid();
        }

        private void rdbTopLayer_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbTopLayer.Checked)
                LayerIndex = 0;
            LoadParamsGenInPrpGrid();
        }

        private void rdbBottomLayer_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbBottomLayer.Checked)
                LayerIndex = 2;
            LoadParamsGenInPrpGrid();
        }
        void LoadParamsGenInPrpGrid()
        {
            if (GenParam[LayerIndex] == null)
                GenParam[LayerIndex] = mChildsGen.CreateDetermParam(SelectedIndexesLayers[LayerIndex]);
            lstbx_GenType.SelectedIndex = SelectedIndexesLayers[LayerIndex];
            GenParam[LayerIndex].Nx = Nx;
            prpGridGen.SelectedObject = GenParam[LayerIndex];


        }

        public AreasProfile2D getArea()
        {
            H = Convert.ToDouble(txtbx_H.Text);
            CorrectNx();

            return
                new AreasProfile2D(L1, L2, L3, H, Nx, mChildsBed.CreateDetermChild(lstbx_BedType.SelectedIndex, BedParam), bottomLayer, topLayer);
        }

        private void prpGridBed_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            AssymerticalDuneBedKwollParam a = new AssymerticalDuneBedKwollParam();
            if (BedParam.Name == a.Name)
            {
                a = BedParam as AssymerticalDuneBedKwollParam;
                L2 = a.CountDunes * 0.9;
                txtbx_L2.Text = L2.ToString();
            }
        }

        private void prpGridGen_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (((prpGridGen.SelectedObject as Parameter).P != 1) && (rdbMainArea.Checked))
            {
                P = (prpGridGen.SelectedObject as Parameter).P;
                Q = (prpGridGen.SelectedObject as Parameter).Q;
                RecalcAllNyDy();
            }
        }

      

        //
        public PointsGeneratorEliz[] getGenerators()
        {
            PointsGeneratorEliz[] generators = new PointsGeneratorEliz[GenParam.Length];
            for (int i = 0; i < GenParam.Length; i++)
            {
                if (GenParam[i] != null)
                    generators[i] = mChildsGen.CreateDetermChild(SelectedIndexesLayers[i], GenParam[i]);
            }

            return generators;
        }
        private void lstbx_BedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BedParam = mChildsBed.CreateDetermParam(lstbx_BedType.SelectedIndex);
            prpGridBed.SelectedObject = BedParam;
            AssymerticalDuneBedKwollParam a = new AssymerticalDuneBedKwollParam();
            if (BedParam.Name == a.Name)
            {
                a = BedParam as AssymerticalDuneBedKwollParam;
                L2 = a.CountDunes * 0.9;
                txtbx_L2.Text = L2.ToString();
                txtbx_L2.Enabled = false;
            }
            else
                txtbx_L2.Enabled = true;
        }

        
        //
        double CalcLAll()
        {
            return L1 + L2 + L3;
        }

        private void txtbx_H_TextChanged(object sender, EventArgs e)
        {
            H = Convert.ToDouble(txtbx_H.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void txtbx_Nx_TextChanged(object sender, EventArgs e)
        {
            CorrectNx();
        }

        private void CorrectNx()
        {
            Nx = Convert.ToInt32(txtbx_Nx.Text);
            if (GenParam[1] != null)
                GenParam[1].Nx = Nx;
            if (GenParam[0] != null)
                GenParam[0].Nx = Nx;
            if (GenParam[2] != null)
                GenParam[2].Nx = Nx;
        }

        private void txtbx_Ny_TextChanged(object sender, EventArgs e)
        {
            NyMiddle = Convert.ToInt32(txtbx_Ny.Text);
            RecalcAllNyDy();
        }

        private void txtbx_L1_TextChanged(object sender, EventArgs e)
        {
            L1 = Convert.ToDouble(txtbx_L1.Text);
            Lbl_Lall.Text = CalcLAll().ToString();
        }

        private void txtbx_L2_TextChanged(object sender, EventArgs e)
        {
            L2 = Convert.ToDouble(txtbx_L2.Text);
            Lbl_Lall.Text = CalcLAll().ToString();
        }

        private void txtbx_L3_TextChanged(object sender, EventArgs e)
        {
            L3 = Convert.ToDouble(txtbx_L3.Text);
            Lbl_Lall.Text = CalcLAll().ToString();
        }

        private void txtbx_TopLayer_TextChanged(object sender, EventArgs e)
        {
            try
            {
                topLayer = Convert.ToDouble(txtbx_TopLayer.Text);
            }
            catch { }

            //
            if (topLayer != 0)
            {
                rdbTopLayer.Visible = true;
                if (GenParam[0] == null)
                    GenParam[0] = mChildsGen.CreateDetermParam(SelectedIndexesLayers[0]);
            }
            else
            {
                rdbTopLayer.Visible = false;
                GenParam[0] = null;
                rdbMainArea.Checked = true;
            }
           
            //
            RecalcAllNyDy();
            CorrectNx();
        }
        private void txtbx_BottomLayer_TextChanged(object sender, EventArgs e)
        {
            try
            {
                bottomLayer = Convert.ToDouble(txtbx_BottomLayer.Text);
            }
            catch { }

            //
            if (bottomLayer != 0)
            {
                rdbBottomLayer.Visible = true;
                if (GenParam[2] == null)
                    GenParam[2] = mChildsGen.CreateDetermParam(SelectedIndexesLayers[2]);
            }
            else
            {
                rdbBottomLayer.Visible = false;
                GenParam[2] = null;
                rdbMainArea.Checked = true;
            }
            //
            
            //
            RecalcAllNyDy();
            CorrectNx();
        }
        void RecalcAllNyDy()
        {
            try
            {
                LineStretch ls = new LineStretch(NyMiddle, P, Q);
                double[] ys = ls.GetCoords(0, Convert.ToDouble(txtbx_H.Text));
                //
                dyTop = ys[0] - ys[1];
                dyBottom = ys[NyMiddle - 2];
                lbl_DyTop.Text = dyTop.ToString("G2", CultureInfo.InvariantCulture);
                lbl_DyBottom.Text = dyBottom.ToString("G2", CultureInfo.InvariantCulture);
                //
                NyTop = Convert.ToInt32(topLayer / dyTop) + 1;
                NyBottom = Convert.ToInt32(bottomLayer / dyBottom) + 1;
                lbl_NyTop.Text = NyTop.ToString();
                lbl_NyBottom.Text = NyBottom.ToString();
                //
                if (GenParam[0] != null)
                    GenParam[0].Ny = NyTop;
                if (GenParam[2] != null)
                    GenParam[2].Ny = NyBottom;
                //
                if (GenParam[1] != null)
                    GenParam[1].Ny = NyMiddle - 2;
                //
                NyAll = NyBottom + NyMiddle + NyTop;
                lblNyAll.Text = NyAll.ToString();
                prpGridGen.Invalidate();
            }
            catch
            { }
        }
    }
}
