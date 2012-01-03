﻿Public Class uSelectElmt
    Dim _retour As String
    Dim _Type As Integer

    Public Event CloseMe(ByVal MyObject As Object)

    Public ReadOnly Property Retour As String
        Get
            Return _retour
        End Get
    End Property
    Public ReadOnly Property Type As Integer
        Get
            Return _Type
        End Get
    End Property

    Public Sub New(ByVal Title As String, ByVal TypeElement As Integer)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        LblTitle.Content = Title

        Select Case TypeElement
            Case 0
                _Type = 0
                For Each driver In Window1.myService.GetAllDrivers(IdSrv)
                    Dim x As New uElement
                    x.ID = driver.ID
                    x.Image = driver.Picture
                    x.Title = driver.Nom
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
            Case 1
                _Type = 1
                For Each device In Window1.myService.GetAllDevices(IdSrv)
                    Dim x As New uElement
                    x.ID = device.ID
                    x.Image = device.Picture
                    x.Title = device.Name
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
            Case 2
                _Type = 2
                For Each zone In Window1.myService.GetAllZones(IdSrv)
                    Dim x As New uElement
                    x.ID = zone.ID
                    x.Image = zone.Icon
                    x.Title = zone.Name
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
            Case 3
                _Type = 3
                For Each user In Window1.myService.GetAllUsers(IdSrv)
                    Dim x As New uElement
                    x.ID = user.ID
                    x.Image = user.Image
                    x.Title = user.Nom
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
            Case 4
                _Type = 4
                For Each trigger In Window1.myService.GetAllTriggers(IdSrv)
                    Dim x As New uElement
                    x.ID = trigger.ID
                    x.Title = trigger.Nom
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
            Case 5
                _Type = 5
                For Each macro In Window1.myService.GetAllMacros(IdSrv)
                    Dim x As New uElement
                    x.ID = macro.ID
                    x.Title = macro.Nom
                    x.Width = 300

                    ListBox1.Items.Add(x)
                Next
        End Select
    End Sub

    Private Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles BtnCancel.Click
        _retour = "CANCEL"
        RaiseEvent CloseMe(Me)
    End Sub

    Private Sub BtnOK_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles BtnOK.Click
        If ListBox1.SelectedItem IsNot Nothing Then
            Dim stk As uElement = ListBox1.SelectedItem
            _retour = stk.ID
            RaiseEvent CloseMe(Me)
        End If
    End Sub

    Private Sub ListBox1_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles ListBox1.MouseDoubleClick
        If ListBox1.SelectedItem IsNot Nothing Then
            Dim stk As uElement = ListBox1.SelectedItem
            _retour = stk.ID
            RaiseEvent CloseMe(Me)
        End If
    End Sub

    Private Sub ListBox1_SelectionChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles ListBox1.SelectionChanged
        For Each Objet As uElement In e.RemovedItems
            Objet.IsSelect = False
        Next
        For Each Objet As uElement In e.AddedItems
            Objet.IsSelect = True
        Next
    End Sub
End Class