using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using PreviewHost.Interop;
using System.Windows.Interop;
using System.Windows.Forms.Integration;
using System.Windows.Forms;

namespace PreviewHost {

    public class PreviewHostControl : WindowsFormsHost, IPreviewHandlerManagedFrame, IDisposable {

        Label label;
        public PreviewHostControl() {
            var panel = new Panel();
            label = new Label {
                Text = "Loading",
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(label);

            Child = panel;
            Loaded += PreviewHostControl_Initialized;
            SizeChanged += PreviewHostControl_SizeChanged;
        }

        private void PreviewHostControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            currentHandler?.ResetBounds();
        }

        private void PreviewHostControl_Initialized(object sender, EventArgs e) {
            if (loadOnFinishedInit != null) {
                UpdatePreview(loadOnFinishedInit);
            }
            loaded = true;
        }

        public string SourcePath {
            get { return (string)GetValue(SourcePathProperty); }
            set { SetValue(SourcePathProperty, value); }
        }

        public IntPtr WindowHandle => Child.Handle;

        public Interop.Rect PreviewerBounds => new Interop.Rect(Child.ClientRectangle);

        public string PreviewStatusText { get => label.Text; private set => label.Text = value; }


        // Using a DependencyProperty as the backing store for SourcePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register("SourcePath", typeof(string), typeof(PreviewHostControl), new PropertyMetadata(null));



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (e.Property == SourcePathProperty) {
                if (!loaded) {
                    loadOnFinishedInit = e.NewValue.ToString();
                } else {
                    UpdatePreview(e.NewValue.ToString());
                }
            } else if (e.Property == ActualHeightProperty || e.Property == ActualWidthProperty) {
                currentHandler?.ResetBounds();
            }
        }

        public void UnloadPreview() {
            PreviewStatusText = "Preview is not loaded.";
            if (currentHandler == null)
                return;
            currentHandler.UnloadPreview();
            currentHandler = null;
        }


        PreviewHandler currentHandler;
        private bool loaded = false;
        private string loadOnFinishedInit;

        private void UpdatePreview(string filePath) {
            var clsid = PreviewHandlerDiscovery.FindPreviewHandlerFor(System.IO.Path.GetExtension(filePath), Child.Handle);
            if (clsid == null) {
                PreviewStatusText = "No preview handler is associated with this file type.";
                return;
            }
            IntPtr pobj = IntPtr.Zero;
            try {
                if (currentHandler != null) {
                    UnloadPreview();
                }
                currentHandler = new PreviewHandler(clsid.Value, this);
                currentHandler.InitWithFileWithEveryWay(filePath);
            } catch (Exception ex) {
                UnloadPreview();
                PreviewStatusText = "Preview handler is malfunctioning:\r\n" + ex.Message;
                return;
            }
            if (Background != null && Background is SolidColorBrush solidColorBrushB) {
                currentHandler.SetBackground(solidColorBrushB.Color);
            }
            if (Foreground != null && Foreground is SolidColorBrush solidColorBrushF) {
                currentHandler.SetForeground(solidColorBrushF.Color);
            }
            currentHandler.DoPreview();
            PreviewStatusText = "";
        }

        public IEnumerable<AcceleratorEntry> GetAcceleratorTable() {
            yield break;
        }

        public bool TranslateAccelerator(ref MSG msg) {
            return false;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            ((IDisposable)currentHandler).Dispose();

        }


    }
}
