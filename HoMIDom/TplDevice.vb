﻿<Serializable()> Public Class TplDevice
    Public Name As String = ""
    Public ID As String = ""
    Public Enable As Boolean = False
    Public DriverId As String = ""
    Public Description As String = ""
    Public Type As String = ""
    Public Adresse1 As String = ""
    Public Adresse2 As String = ""
    Public DateCreated As Date = Nothing
    Public LastChange As Date = Nothing
    Public LastChangeDuree As Integer = 0
    Public Refresh As Double = 0
    Public Modele As String = ""
    Public Picture As String = ""
    Public Solo As Boolean = True
    Public LastEtat As Boolean = False
    Public Value As Double = 0
    Public ValueLast As Double = 0
    Public ValueMin As Double = -9999
    Public ValueMax As Double = 9999
    Public ValueDef As Double = 0
    Public Precision As Double = 0
    Public Correction As Double = 0
    Public Formatage As String = ""
    Public ListCommandName As New List(Of String)
    Public ListCommandData As New List(Of String)
    Public ListCommandRepeat As New List(Of String)
End Class