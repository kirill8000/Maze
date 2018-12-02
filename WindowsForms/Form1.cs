using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms
{
    public partial class Form1 : Form
    {
        private Maze _maze;
        public Form1()
        {
            InitializeComponent();
            picture.SizeMode = PictureBoxSizeMode.Zoom;
            stepCheckBox.CheckedChanged += (sender, args) => { trackBar1.Enabled = stepCheckBox.Checked; };
        }
          
        private async void generateButton_Click(object sender, EventArgs e)
        {
            genrateButton.Enabled = false;
            _maze = new Maze((xTrackBar.Value) * 10 + 1, (yTrackBar.Value) * 10 + 1, new Random());
            picture.Image = await _maze.GetBitmapAsync();
            solveButton.Enabled = true;
            clearButton.Enabled = false;
            genrateButton.Enabled = true;
        }

        private async void solveButton_Click(object sender, EventArgs e)
        {
            solveButton.Enabled = false;
            clearButton.Enabled = false;
            genrateButton.Enabled = false;
            if (!stepCheckBox.Checked)
            {
                picture.Image = await _maze.GetSolveBitmapAsync();
                return;
            }
            foreach (var bitmap in _maze.GetSolveTrack())
            {
                if (!stepCheckBox.Checked)
                {
                    picture.Image = await _maze.GetSolveBitmapAsync();
                    break;
                }
                picture.Image = bitmap;
                await Task.Delay(1000 / trackBar1.Value);
            }

            solveButton.Enabled = true;
            clearButton.Enabled = true;
            genrateButton.Enabled = true;
        }

        private async void clearButton_Click(object sender, EventArgs e)
        {
            picture.Image = await _maze.GetBitmapAsync();
            clearButton.Enabled = false;
        }
    }
}
