'
' This work is licensed under a Creative Commons Attribution 3.0 Unported License.
'
' Thomas Dideriksen (thomas@dideriksen.com)
'

Imports System.IO
Imports System.Windows.Forms
Imports Nikon

Public Class Form

    Private manager As NikonManager
    Private device As NikonDevice
    Private liveViewTimer As Timer

    Protected Overrides Sub OnLoad(e As System.EventArgs)

        ' Disable buttons
        ToggleButtons(False)

        'Initialize live view timer
        liveViewTimer = New Timer()
        liveViewTimer.Interval = 1000 / 30
        AddHandler liveViewTimer.Tick, AddressOf liveViewTimer_Tick

        ' Initialize Nikon manager
        manager = New NikonManager("Type0003.md3")
        AddHandler manager.DeviceAdded, AddressOf OnDeviceAdded
        AddHandler manager.DeviceRemoved, AddressOf OnDeviceRemoved

        MyBase.OnLoad(e)

    End Sub

    Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)

        ' Disable live view (in case it's enabled)
        If Not device Is Nothing Then
            device.LiveViewEnabled = False
        End If

        ' Shut down the Nikon manager
        manager.Shutdown()

        MyBase.OnClosing(e)

    End Sub

    Private Sub OnDeviceAdded(sender As NikonManager, device As NikonDevice)

        Me.device = device

        ' Set the device name
        Label_Name.Text = device.Name

        ' Enable buttons
        ToggleButtons(True)

        ' Hook up device capture events
        AddHandler device.ImageReady, AddressOf OnImageReady
        AddHandler device.CaptureComplete, AddressOf OnCaptureComplete

    End Sub

    Private Sub OnDeviceRemoved(sender As NikonManager, device As NikonDevice)

        Me.device = Nothing

        ' Stop live view timer
        liveViewTimer.Stop()

        ' Clear device name
        Label_Name.Text = "No Camera"

        ' Disable buttons
        ToggleButtons(False)

        ' Clear live view picture
        PictureBox.Image = Nothing

    End Sub

    Private Sub liveViewTimer_Tick(sender As Object, e As EventArgs)

        ' Get live view image
        Dim image As NikonLiveViewImage = Nothing

        Try

            image = device.GetLiveViewImage()

        Catch ex As NikonException

            liveViewTimer.Stop()

        End Try

        ' Set live view image on picture box
        If Not image Is Nothing Then

            Dim stream As MemoryStream = New MemoryStream(image.JpegBuffer)
            PictureBox.Image = System.Drawing.Image.FromStream(stream)

        End If

    End Sub

    Private Sub OnImageReady(sender As NikonDevice, image As NikonImage)

        Dim dialog As SaveFileDialog = New SaveFileDialog()

        If (image.Type = NikonImageType.Jpeg) Then

            dialog.Filter = "Jpeg Image (*.jpg)|*.jpg"

        Else

            dialog.Filter = "Nikon NEF (*.nef)|*.nef"

        End If

        If dialog.ShowDialog() = Windows.Forms.DialogResult.OK Then

            Dim stream As FileStream = New FileStream(dialog.FileName, FileMode.Create, FileAccess.Write)
            stream.Write(image.Buffer, 0, image.Buffer.Length)
            stream.Close()

        End If

    End Sub

    Private Sub OnCaptureComplete(sender As NikonDevice, data As Integer)

        ' Re-enable buttons when the capture completes
        ToggleButtons(True)

    End Sub

    Private Sub ToggleButtons(toggle As Boolean)
        Button_Capture.Enabled = toggle
        Button_ToggleLiveView.Enabled = toggle
    End Sub

    Private Sub Button_Capture_Click(sender As System.Object, e As System.EventArgs) Handles Button_Capture.Click

        If device Is Nothing Then
            Return
        End If

        ToggleButtons(False)

        Try
            device.Capture()
        Catch ex As NikonException
            MsgBox(ex.Message)
            ToggleButtons(True)
        End Try

        PictureBox.Image = Nothing

    End Sub

    Private Sub Button_ToggleLiveView_Click(sender As System.Object, e As System.EventArgs) Handles Button_ToggleLiveView.Click

        If device Is Nothing Then
            Return
        End If

        If device.LiveViewEnabled Then

            device.LiveViewEnabled = False
            liveViewTimer.Stop()
            PictureBox.Image = Nothing

        Else

            device.LiveViewEnabled = True
            liveViewTimer.Start()

        End If

    End Sub

End Class
