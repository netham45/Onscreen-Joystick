Public Class controlStick
    Implements controlBase

    Declare Sub keybd_event Lib "user32" (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer, ByVal dwExtraInfo As Integer)

    Dim X As Integer
    Dim Y As Integer
    Dim pointerX As Integer
    Dim pointerY As Integer
    Dim isPointerOn As Boolean = False
    Dim inputID = -1
    Dim outerRect As Rectangle
    Dim pointerRect As Rectangle
    Dim HotSpots As HotSpot()

    Sub New(X As Integer, Y As Integer)
        Me.X = X
        Me.Y = Y
        Me.outerRect = New Rectangle(X, Y, 200, 200)
        Me.pointerRect = New Rectangle(0, 0, 50, 50)
        pointerX = outerRect.X + (outerRect.Width / 2) 'Center the pointer
        pointerY = outerRect.Y + (outerRect.Height / 2)
        ReDim HotSpots(0)
    End Sub

    Sub addHotSpot(lowerBound As Integer, upperBound As Integer, virtualKey As Integer)
        Dim newSpot As HotSpot
        newSpot.lowerBound = lowerBound
        newSpot.upperBound = upperBound
        newSpot.virtualKey = virtualKey
        newSpot.isPressed = False
        ReDim Preserve HotSpots(HotSpots.Length)
        HotSpots(HotSpots.Length - 1) = newSpot
        handleKeyPresses() 'Update the currently hit keys
    End Sub

    Sub paint(g As System.Drawing.Graphics) Implements controlBase.paint
        clampPointerDistance()
        pointerRect.X = pointerX - (pointerRect.Width / 2)
        pointerRect.Y = pointerY - (pointerRect.Height / 2)
        Dim b As SolidBrush = New SolidBrush(Color.FromArgb(64, 100, 100, 100))
        g.FillEllipse(b, outerRect)
        g.DrawEllipse(Pens.Black, outerRect)
        b = New SolidBrush(Color.FromArgb(IIf(isPointerOn, 192, 64), 100, 100, 100))
        g.FillEllipse(b, pointerRect)
        g.DrawEllipse(Pens.Black, pointerRect)
    End Sub

    Sub keyPressAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyPressAt
        If inputID = -1 Then
            inputID = touchID
            isPointerOn = True
            pointerX = X
            pointerY = Y
            handleKeyPresses()
        End If
    End Sub

    Function keyMoveAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyMoveAt
        If inputID = touchID Then
            isPointerOn = True
            pointerX = X
            pointerY = Y
            handleKeyPresses()
            Return True
        End If
        Return False
    End Function

    Sub keyReleaseAt(touchID As Integer, X As Integer, Y As Integer) Implements controlBase.keyReleaseAt
        If touchID = inputID Then
            isPointerOn = False
            inputID = -1
            pointerX = outerRect.X + (outerRect.Width / 2) 'Center the pointer
            pointerY = outerRect.Y + (outerRect.Height / 2)
            handleKeyPresses()
        End If
    End Sub

    Sub clampPointerDistance()
        Dim outerRadius As Double = (outerRect.Width / 2)
        Dim x As Double = pointerX - (outerRect.X + outerRadius)
        Dim y As Double = pointerY - (outerRect.Y + outerRadius)
        Dim distance As Double = Math.Sqrt(x * x + y * y)
        Dim maxDistance As Double = outerRadius - (pointerRect.Width / 2)
        If distance > maxDistance Then
            If (x = 0) Then
                pointerX = (outerRect.X + outerRadius)
                pointerY = Math.Floor((pointerY / distance) * maxDistance) + (outerRect.Y + outerRadius)
            Else
                Dim line As Double = y / x
                Dim newX As Double = IIf(x >= 0, 1, -1) * maxDistance / Math.Sqrt((line * line) + 1)
                Dim newY As Double = newX * line
                pointerX = Math.Floor(newX) + (outerRect.X + outerRadius)
                pointerY = Math.Floor(newY) + (outerRect.Y + outerRadius)
            End If
        End If
    End Sub

    Sub handleKeyPresses()
        If Not pointerX = outerRect.X + (outerRect.Width / 2) Or pointerY = outerRect.Y + (outerRect.Height / 2) Then
            'Down = 0
            'Right = 90
            'Top = 180
            'Left = 270
            Dim outerRadius As Double = (outerRect.Width / 2)
            Dim x As Double = pointerX - (outerRect.X + outerRadius)
            Dim y As Double = pointerY - (outerRect.Y + outerRadius)
            Dim distance As Double = Math.Sqrt(x * x + y * y)
            Dim Degrees As Double = IIf(y < 0, 180, 0) + (Math.Atan(x / y) * 180 / 3.14159)
            If Degrees < 0 Then
                Degrees += 360
            End If
            If System.Double.IsNaN(Degrees) Then
                Degrees = 0
            End If

            For i As Integer = 0 To HotSpots.Length - 1
                If IIf(HotSpots(i).upperBound > HotSpots(i).lowerBound, _
                       HotSpots(i).upperBound >= Degrees And HotSpots(i).lowerBound <= Degrees, _
                       HotSpots(i).upperBound >= Degrees Or HotSpots(i).lowerBound <= Degrees) And distance >= 10 Then 'Check if it's in bounds.
                    If Not HotSpots(i).isPressed Then
                        HotSpots(i).isPressed = True
                        keybd_event(HotSpots(i).virtualKey, 0, 1, 0)
                    End If
                Else
                    If HotSpots(i).isPressed Then
                        HotSpots(i).isPressed = False
                        keybd_event(HotSpots(i).virtualKey, 0, 3, 0)
                    End If
                End If
            Next
        End If
    End Sub

    Function contains(X As Integer, Y As Integer) Implements controlBase.contains
        Return outerRect.Contains(X, Y)
    End Function

End Class