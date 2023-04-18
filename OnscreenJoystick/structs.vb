Public Structure TouchEvent
    Dim X As Integer
    Dim Y As Integer
    Dim hSource As IntPtr
    Dim ID As Integer
    Dim Flags As Integer
    Dim Mask As Integer
    Dim Time As Integer
    Dim pExtraInfo As IntPtr
    Dim xContact As Integer
    Dim yContact As Integer
End Structure

Structure HotSpot
    Dim lowerBound As Integer
    Dim upperBound As Integer
    Dim virtualKey As Integer
    Dim isPressed As Boolean
End Structure