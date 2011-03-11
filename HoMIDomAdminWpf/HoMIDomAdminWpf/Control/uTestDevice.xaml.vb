﻿Imports HoMIDom.HoMIDom.Api

Partial Public Class uTestDevice
    Dim _DeviceId As String
    Dim _Device As Object
    Dim _list As New List(Of String)

    Public Event CloseMe(ByVal MyObject As Object)

    Private Sub BtnTest_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles BtnTest.Click
        Try
            Dim _Param As New ArrayList

            If TxtP1.Text <> "" Then _Param.Add(TxtP1.Text)
            If TxtP2.Text <> "" Then _Param.Add(TxtP2.Text)
            If TxtP3.Text <> "" Then _Param.Add(TxtP3.Text)
            If TxtP4.Text <> "" Then _Param.Add(TxtP4.Text)
            If TxtP5.Text <> "" Then _Param.Add(TxtP5.Text)

            Window1.myService.ExecuteDeviceCommand(_DeviceId, CbCmd.Text, _Param)
        Catch ex As Exception
            MessageBox.Show("Erreur lors du test: " & ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Public Sub New(ByVal DeviceId As String)

        ' Cet appel est requis par le Concepteur Windows Form.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        _DeviceId = DeviceId

        _Device = Window1.myService.ReturnDeviceByID(_DeviceId)

        If _Device IsNot Nothing Then

            _list = Window1.myService.ListMethod(_DeviceId)

            For i As Integer = 0 To _list.Count - 1
                Dim a() As String
                a = _list(i).split("|")
                If a.Length > 0 Then
                    CbCmd.Items.Add(a(0))
                End If
                If a.Length > 1 Then 'il y a des paramètres
                    If a.Length > 6 Then
                        MessageBox.Show("Seuls 5 paramètres sont acceptés veuillez contacter l'administrateur !!", "Erreur", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                        RaiseEvent CloseMe(Me)
                    End If
                End If
            Next
        Else
            MessageBox.Show("Le Device est inconnu !!", "Erreur", MessageBoxButton.OK, MessageBoxImage.Exclamation)
            RaiseEvent CloseMe(Me)
        End If

    End Sub

    Private Sub BtnClose_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles BtnClose.Click
        RaiseEvent CloseMe(Me)
    End Sub

    Private Sub CbCmd_MouseLeave(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles CbCmd.MouseLeave
        LblP1.Visibility = Windows.Visibility.Hidden
        LblP2.Visibility = Windows.Visibility.Hidden
        LblP3.Visibility = Windows.Visibility.Hidden
        LblP4.Visibility = Windows.Visibility.Hidden
        LblP5.Visibility = Windows.Visibility.Hidden
        TxtP1.Text = ""
        TxtP2.Text = ""
        TxtP3.Text = ""
        TxtP4.Text = ""
        TxtP5.Text = ""
        TxtP1.Visibility = Windows.Visibility.Hidden
        TxtP2.Visibility = Windows.Visibility.Hidden
        TxtP3.Visibility = Windows.Visibility.Hidden
        TxtP4.Visibility = Windows.Visibility.Hidden
        TxtP5.Visibility = Windows.Visibility.Hidden

        If CbCmd.Text = "" Then Exit Sub
        If CbCmd.SelectedIndex < 0 Then Exit Sub

        Dim Idx As Integer = CbCmd.SelectedIndex
        Dim a() As String = _list(Idx).split("|")
        If a.Length > 1 Then
            For j As Integer = 1 To a.Length - 1

                Select Case j
                    Case 1
                        Dim _tmp() As String = a(j).Split(":")
                        LblP1.Content = _tmp(0) & " :"
                        LblP1.Visibility = Windows.Visibility.Visible
                        TxtP1.ToolTip = _tmp(1)
                        TxtP1.Visibility = Windows.Visibility.Visible
                    Case 2
                        Dim _tmp() As String = a(j).Split(":")
                        LblP2.Content = _tmp(0) & " :"
                        LblP2.Visibility = Windows.Visibility.Visible
                        TxtP2.ToolTip = _tmp(1)
                        TxtP2.Visibility = Windows.Visibility.Visible
                    Case 3
                        Dim _tmp() As String = a(j).Split(":")
                        LblP3.Content = _tmp(0) & " :"
                        LblP3.Visibility = Windows.Visibility.Visible
                        TxtP3.ToolTip = _tmp(1)
                        TxtP3.Visibility = Windows.Visibility.Visible
                    Case 4
                        Dim _tmp() As String = a(j).Split(":")
                        LblP4.Content = _tmp(0) & " :"
                        LblP4.Visibility = Windows.Visibility.Visible
                        TxtP4.ToolTip = _tmp(1)
                        TxtP4.Visibility = Windows.Visibility.Visible
                    Case 5
                        Dim _tmp() As String = a(j).Split(":")
                        LblP5.Content = _tmp(0) & " :"
                        LblP5.Visibility = Windows.Visibility.Visible
                        TxtP5.ToolTip = _tmp(1)
                        TxtP5.Visibility = Windows.Visibility.Visible
                End Select
            Next
        End If
    End Sub

End Class