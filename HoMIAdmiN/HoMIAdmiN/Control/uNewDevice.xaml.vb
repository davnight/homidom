﻿Imports System.Data
Imports System.IO
Imports System.Collections.ObjectModel

Public Class uNewDevice
    Public Event CloseMe(ByVal MyObject As Object)

    Private Sub BtnClose_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles BtnClose.Click
        RaiseEvent CloseMe(Me)
    End Sub

    Public Sub New()
        Try

            ' Cet appel est requis par le Concepteur Windows Form.
            InitializeComponent()

            ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
            DGW.ItemsSource = myService.GetAllNewDevice
            AddHandler DGW.Loaded, AddressOf GridOk

            'RefreshGrid()
        Catch ex As Exception
            MessageBox.Show("Erreur lors sur la fonction New de uNewDevice: " & ex.ToString, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try

    End Sub

    Private Sub GridOk(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Try
            'Do While DGW.Columns.Count > 2
            '    DGW.Columns.RemoveAt(0)
            'Loop

        Catch ex As Exception
            MessageBox.Show("Erreur lors de l'affichage du relevé GridOK: " & ex.ToString, "Erreur Admin", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub RefreshGrid()
        Try
            Me.Cursor = Nothing
        Catch ex As Exception
            MessageBox.Show("Erreur lors de la récuppération du RefreshGrid: " & ex.ToString, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

End Class