Public Class controlButton
    Implements controlBase
    Declare Sub keybd_event Lib "user32" (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)

    Dim btnText As String
    Dim virtKey As Integer
    Dim X As Integer
    Dim Y As Integer
    Dim drawRect As Rectangle
    Private isKeyDown As Boolean
    Private inputID As Integer = -1

    Sub New(X As Integer, Y As Integer, virtKey As Integer, btnText As String)
        Me.virtKey = virtKey
        Me.btnText = btnText
        Me.X = X
        Me.Y = Y
        Me.drawRect = New Rectangle(X, Y, 50, 50)
    End Sub

    Sub paint(g As System.Drawing.Graphics) Implements controlBase.paint
        Dim b As SolidBrush = New SolidBrush(Color.FromArgb(IIf(isKeyDown, 192, 64), 100, 100, 100))
        g.FillEllipse(b, drawRect)
        g.DrawEllipse(Pens.Black, drawRect)
        g.DrawString(btnText, New Font("Arial", 16), Brushes.Black, New Point(X + 15, Y + 14))
    End Sub

    Sub keyPressAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyPressAt
        If inputID = -1 Then
            isKeyDown = True
            keybd_event(virtKey, 0, 1, 0)
            inputID = touchID
            'trace.WriteLine("Button down")
        End If
    End Sub

    Function keyMoveAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyMoveAt
        Return False
    End Function

    Sub keyReleaseAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyReleaseAt
        If touchID = inputID Then
            inputID = -1
            isKeyDown = False
            keybd_event(virtKey, 0, 3, 0) '2 = KEYEVENTF_KEYUP
            'trace.WriteLine("Button up")
        End If
    End Sub

    Function contains(X As Integer, Y As Integer) Implements controlBase.contains
        Return drawRect.Contains(X, Y)
    End Function

End Class
