Public Interface controlBase
    'Gets called with Form1.paint
    Sub paint(g As System.Drawing.Graphics)
    'This gets called when a key gets pressed
    Sub keyPressAt(touchID As Integer, X As Integer, Y As Integer)
    'This will get called when the pressed key moves, this returns true if Form1 should redraw.
    Function keyMoveAt(touchID As Integer, X As Integer, Y As Integer)
    'This gets called when the pressed key releases
    Sub keyReleaseAt(touchID As Integer, X As Integer, Y As Integer)
    'Lets us know if it contains a point
    Function contains(X As Integer, Y As Integer)
End Interface