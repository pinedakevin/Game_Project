using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GC;
using TankWars;

namespace View
{
    /// <summary>
    /// Represents the GUI for the tankWars client
    /// </summary>
    public partial class TankWarsGUI : Form
    {
        /// <summary>
        /// World is a simple container for Players and Powerups
        // The controller owns the world, but we have a reference to it
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Represents the players name that will be 
        /// displayed as the game is played
        /// </summary>
        public string playerName { get; private set; }

        /// <summary>
        /// Represents the panel in which the game will be drawn
        /// </summary>
        DrawingPanel drawingPanel;

        /// <summary>
        /// The controller handles updates from the "server"
        /// and notifies us via an event
        /// </summary>
        GameController controller;

        /// <summary>
        /// Represents the Size of the view that contains 
        /// the drawing panel for the game
        /// </summary>
        private const int viewSize = 900;

        /// <summary>
        /// Represents the top area of the game where there is
        /// important functionality to connect to the server
        /// </summary>
        private const int menuSize = 40;

        /// <summary>
        /// Represents the commands to control the tank
        /// </summary>
        public ControlCommands ctrlcmd;

        /// <summary>
        /// Constructor for the GUI
        /// </summary>
        /// <param name="ctl">the controller</param>
        public TankWarsGUI(GameController ctl)
        {
            InitializeComponent();

            //setting the controller and the size of the client
            controller = ctl;
            theWorld = controller.GetWorld();
            ctrlcmd = controller.ctrlcmd;
            ClientSize = new Size(viewSize, viewSize + menuSize);


            // prefilled the text boxes
            playerNameBox.Text = "Jugador";
            serverAddressBox.Text = "localhost";

            playerName = playerNameBox.Text;
            playerNameBox.Enabled = true;
            serverAddressBox.Enabled = true;

            //set up controller and form handlers
            controller.UpdateArrived += OnFrame;
            controller.Error += OnError;
            FormClosed += OnExit;

            // Placing and adding the drawing panel
            drawingPanel = new DrawingPanel(theWorld, controller);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

            //Set up key and mouse handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMoved;
        }

       
        /// <summary>
        /// Event handler that closes the conexion smoothly
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e"></param>
        private void OnExit(object sender, FormClosedEventArgs e)
        {
            controller.Close();
        }
        /// <summary>
        /// Event handler that displays a window when an error has occurred
        /// </summary>
        /// <param name="err"></param>
        private void OnError(string err)
        {
            MessageBox.Show(err);
        }

        /// <summary>
        /// Handler for the controller's UpdateArrived event
        /// </summary>
        private void OnFrame()
        {
            // Invalidate this form and all its children
            try
            {
                Invoke(new MethodInvoker(() => Invalidate(true)));
            }
            catch (ObjectDisposedException ) 
            {
                controller.Close();
            }
        }

        /// <summary>
        /// Handles the connection to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            if (serverAddressBox.Text.Length > 0 && playerNameBox.Text.Length > 0)
            {
                if (playerNameBox.Text.Length <= 16)
                {
                    connectButton.Enabled = false;
                    // Enable the global form to capture key presses
                    KeyPreview = true;
                    controller.Connect(playerNameBox.Text, serverAddressBox.Text);
                }

                else
                {
                    MessageBox.Show("Your name should be less than 16 characters or less");
                }
            }
            else
            {
                MessageBox.Show("You must assign a server and/or a name.");
            }
        }


        /// <summary>
        /// Keys handler for W,S,A,D keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {       
            string informControl = null;
            switch (e.KeyCode)
            {
                case Keys.W:
                    informControl = "up";
                    break;
                case Keys.S:
                    informControl = "down";
                    break;
                case Keys.A:
                    informControl = "left";
                    break;
                case Keys.D:
                    informControl = "right";
                    break;
                default:
                    // code block
                    break;
            }
            controller.HandleMoveRequest(informControl);
            
            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }


        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            string informControl = null;
            switch (e.KeyCode)
            {
                case Keys.W:
                    informControl = "up";
                    break;
                case Keys.S:
                    informControl = "down";
                    break;
                case Keys.A:
                    informControl = "left";
                    break;
                case Keys.D:
                    informControl = "right";
                    break;
                default:
                    // code block
                    break;
            }
            controller.CancelMoveRequest(informControl);
        }

        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            string informControl = null;
            if (e.Button == MouseButtons.Left) informControl = "left";
            else if (e.Button == MouseButtons.Right) informControl = "right";

                controller.HandleMouseRequest(informControl);
        }

        /// <summary>
        /// Handle mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            string informControl = null;
            if (e.Button == MouseButtons.Left) informControl = "left";
            else if (e.Button == MouseButtons.Right) informControl = "right";

            controller.CancelMouseRequest(informControl);
        }

        /// <summary>
        /// Handler when user moves the mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMoved(object sender, MouseEventArgs e)
        {
            double turrentX = e.X - viewSize / 2;
            double turrentY = e.Y - viewSize / 2;
            ctrlcmd.turretDirection = new Vector2D(turrentX, turrentY);
            string movingMouse = "moving";
            controller.MouseMoved(movingMouse,ctrlcmd.turretDirection);
        }

    }
}
