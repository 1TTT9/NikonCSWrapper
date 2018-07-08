<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox = New System.Windows.Forms.PictureBox()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.Label_Name = New System.Windows.Forms.Label()
        Me.Button_Capture = New System.Windows.Forms.Button()
        Me.Button_ToggleLiveView = New System.Windows.Forms.Button()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.Controls.Add(Me.PictureBox, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel2, 0, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(624, 442)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'PictureBox
        '
        Me.PictureBox.BackColor = System.Drawing.SystemColors.ControlDark
        Me.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox.Location = New System.Drawing.Point(153, 3)
        Me.PictureBox.Name = "PictureBox"
        Me.PictureBox.Size = New System.Drawing.Size(511, 436)
        Me.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox.TabIndex = 0
        Me.PictureBox.TabStop = False
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 1
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.Controls.Add(Me.Label_Name, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Button_Capture, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.Button_ToggleLiveView, 0, 2)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(3, 3)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 4
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(144, 436)
        Me.TableLayoutPanel2.TabIndex = 1
        '
        'Label_Name
        '
        Me.Label_Name.AutoSize = True
        Me.Label_Name.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label_Name.Location = New System.Drawing.Point(3, 0)
        Me.Label_Name.Name = "Label_Name"
        Me.Label_Name.Size = New System.Drawing.Size(138, 40)
        Me.Label_Name.TabIndex = 0
        Me.Label_Name.Text = "No Camera"
        Me.Label_Name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Button_Capture
        '
        Me.Button_Capture.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_Capture.Location = New System.Drawing.Point(3, 43)
        Me.Button_Capture.Name = "Button_Capture"
        Me.Button_Capture.Size = New System.Drawing.Size(138, 34)
        Me.Button_Capture.TabIndex = 1
        Me.Button_Capture.Text = "Capture"
        Me.Button_Capture.UseVisualStyleBackColor = True
        '
        'Button_ToggleLiveView
        '
        Me.Button_ToggleLiveView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Button_ToggleLiveView.Location = New System.Drawing.Point(3, 83)
        Me.Button_ToggleLiveView.Name = "Button_ToggleLiveView"
        Me.Button_ToggleLiveView.Size = New System.Drawing.Size(138, 34)
        Me.Button_ToggleLiveView.TabIndex = 2
        Me.Button_ToggleLiveView.Text = "Toggle Live View"
        Me.Button_ToggleLiveView.UseVisualStyleBackColor = True
        '
        'Form
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(624, 442)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "Form"
        Me.Text = "Demo WinForms VB.NET"
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.PictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents PictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Label_Name As System.Windows.Forms.Label
    Friend WithEvents Button_Capture As System.Windows.Forms.Button
    Friend WithEvents Button_ToggleLiveView As System.Windows.Forms.Button

End Class
