
Public Class frmOnscreenJoystick
    Private Declare Function RegisterTouchWindow Lib "User32.dll" (ByVal hwnd As IntPtr, ByVal ulFlags As Integer) As Boolean
    Private Declare Function GetTouchInputInfo Lib "User32.dll" (ByVal HTOUCHINPUT As IntPtr, ByVal cInputs As UInteger, ByRef PTOUCHINPUT As TouchEvent, ByVal cbSize As Integer) As Boolean
    Private Declare Function SetActiveWindow Lib "User32.dll" (ByVal hwnd As IntPtr) As IntPtr
    Private Declare Function SetWindowLong Lib "user32.dll" Alias "SetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Long
    Private Declare Auto Function GetWindowLong Lib "User32.Dll" (ByVal hWnd As System.IntPtr, ByVal nIndex As Integer) As Integer

    Dim osjControls As ArrayList = New ArrayList()
    Dim doNextRefresh As Boolean = False

    Private Sub frmOnscreenJoystick_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'Set properties
        Me.TopMost = True
        SetWindowLong(Me.Handle, -20, GetWindowLong(Me.Handle, -20) Or &H8000000) 'This makes us not take focus on click. -20 = GWL_EXSTYLE, &H8000000 = WS_EX_NOACTIVATE
        Me.Width = Screen.PrimaryScreen.Bounds.Width
        Me.Height = Screen.PrimaryScreen.Bounds.Height
        Me.Left = 0
        Me.Top = 0
        Me.Opacity = 0.5
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)

    End Sub

    Private Sub timerResolutionCheck_Tick(sender As Object, e As EventArgs) Handles timerResolutionCheck.Tick
        Me.Width = Screen.PrimaryScreen.Bounds.Width
        Me.Height = Screen.PrimaryScreen.Bounds.Height
        Me.Left = 0
        Me.Top = 0
        Me.Opacity = 0.5
    End Sub

    Private Sub frmOnscreenJoystick_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not IO.File.Exists("OSJConfig.cfg") Then
            MsgBox("Please create OSJConfig.cfg.")
            Application.Exit()
        Else
            Dim Lines As String() = IO.File.ReadAllLines("OSJConfig.cfg")
            Dim curStick As controlStick = Nothing
            Dim curLine As Integer = 0
            For Each Line As String In Lines
                curLine += 1
                If Line.StartsWith(";") Then
                    Continue For
                End If
                Dim Parts As String() = Line.Split(" ")
                If Line.StartsWith("BUTTON") Then
                    osjControls.Add(New controlButton(Integer.Parse(Parts(1)), Integer.Parse(Parts(2)), Parts(3), Parts(4)))
                End If
                If Line.StartsWith("STICK") Then
                    curStick = New controlStick(Integer.Parse(Parts(1)), Integer.Parse(Parts(2)))
                End If
                If Line.StartsWith("HOTSPOT") Then
                    If IsNothing(curStick) Then
                        MsgBox("Error on line " & curLine & ": Create a stick with STICK X Y before adding a HOTSPOT. Ignoring HOTSPOT!")
                        Continue For
                    Else
                        curStick.addHotSpot(Parts(1), Parts(2), Parts(3))
                    End If
                End If
                If Line.StartsWith("ENDSTICK") Then
                    osjControls.Add(curStick)
                    curStick = Nothing
                End If
            Next
            If Not IsNothing(curStick) Then
                MsgBox("Error: Last STICK not closed with ENDSTICK directive")
            End If
        End If

        RegisterTouchWindow(Me.Handle, 0)
    End Sub

    Private Sub frmOnscreenJoystick_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        For Each osjControl As controlBase In osjControls
            osjControl.keyReleaseAt(-1, e.X, e.Y)
        Next
        Me.Refresh()
    End Sub

    Private Sub frmOnscreenJoystick_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        For Each osjControl As controlBase In osjControls
            osjControl.keyPressAt(-1, e.X, e.Y)
        Next
        Me.Refresh()
    End Sub

    Private Sub frmOnscreenJoystick_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        e.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        For Each osjControl As controlBase In osjControls
            osjControl.paint(e.Graphics)
        Next
    End Sub

    Protected Overrides Sub WndProc(ByRef recWinMessage As System.Windows.Forms.Message)
        If recWinMessage.Msg = &H240 Then 'WM_TOUCH
            Dim inputs As TouchEvent()
            ReDim inputs(recWinMessage.WParam)
            GetTouchInputInfo(recWinMessage.LParam, recWinMessage.WParam, inputs(0), 40)
            For Each input As TouchEvent In inputs
                input.X /= 100
                input.Y /= 100
                input.Flags = input.Flags And &HF 'We only care about the lower byte.
                ''trace.WriteLine(input.X & "," & input.Y & " ID:" & input.ID & " Flags:" & input.Flags)

                If input.Flags = 9 Then 'Move
                    Dim doRefresh As Boolean = False
                    For Each osjControl As controlBase In osjControls
                        If (osjControl.keyMoveAt(input.ID, input.X, input.Y)) Then
                            doRefresh = True
                        End If
                    Next
                    If doRefresh Then
                        doNextRefresh = True
                    End If
                ElseIf input.Flags = 10 Then 'Touch
                    For Each osjControl As controlBase In osjControls
                        If osjControl.contains(input.X, input.Y) Then
                            osjControl.keyPressAt(input.ID, input.X, input.Y)
                        End If
                    Next
                    doNextRefresh = True
                ElseIf input.Flags = 4 Then 'Release
                    For Each osjControl As controlBase In osjControls
                        osjControl.keyReleaseAt(input.ID, input.X, input.Y)
                    Next
                    doNextRefresh = True
                End If

            Next
        ElseIf recWinMessage.Msg = &H2CC Then 'WM_TABLET_QUERYSYSTEMGESTURESTATUS
            recWinMessage.Result = &H1000001 'TABLET_DISABLE_PRESSANDHOLD  | TABLET_ENABLE_MULTITOUCHDATA
        ElseIf recWinMessage.Msg = &H21 Then 'WM_MOUSEACTIVATE
            recWinMessage.Result = &H4 'MA_NOACTIVATEEAT (This kills mouse events)
        Else
            MyBase.WndProc(recWinMessage)
        End If

    End Sub

    Private Sub timerRefresh_Tick(sender As Object, e As EventArgs) Handles timerRefresh.Tick
        If doNextRefresh Then
            Me.Refresh()
            doNextRefresh = False
        End If

    End Sub
End Class
