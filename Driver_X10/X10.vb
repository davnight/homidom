﻿Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.IO.Ports
Imports System.Math
Imports System.Net.Sockets
Imports System.Threading
Imports System.Globalization

Public Class x10
    Public WithEvents port As New System.IO.Ports.SerialPort
    Private port_ouvert As Boolean = False
    Private port_name As String = ""
    Private com_to_hex As New Dictionary(Of String, Byte)
    Private house_to_hex As New Dictionary(Of String, Byte)
    Private device_to_hex As New Dictionary(Of String, Byte)
    Private GetPortInput As Boolean
    Private OutPortDevice As Boolean = False

#Region "Déclaration"
    ' CM11 Handshaking codes
    Public Const SET_TIME As Byte = &H9B
    Public Const INFERFACE_READY As Byte = &H55
    Public Const COMPUTER_READY As Byte = &HC3
    Public Const ACK As Byte = &H0

    ' CM11 Hail codes
    Public Const INTERFACE_CQ As Byte = &H5A
    Public Const CM11_CLOCK_REQ As Byte = &HA5
    Public Const CP10_CLOCK_REQ As Byte = &HA6
    Public Const ACK_DEF_CM11 As Byte = &HFB

    'CM11 Command Header Definitions
    Public Const STANDARD_ADDRESS As Byte = &H4
    Public Const STANDARD_FUNCTION As Byte = &H6
    Public Const ENHANCED_ADDRESS As Byte = &H5
    Public Const ENHANCED_FUNCTION As Byte = &H7

    Private BufferIn(8192) As Byte

#End Region

    Public Sub New()

        house_to_hex.Add("A", &H60)
        house_to_hex.Add("B", &HE0)
        house_to_hex.Add("C", &H20)
        house_to_hex.Add("D", &HA0)
        house_to_hex.Add("E", &H10)
        house_to_hex.Add("F", &H90)
        house_to_hex.Add("G", &H50)
        house_to_hex.Add("H", &HD0)
        house_to_hex.Add("I", &H70)
        house_to_hex.Add("J", &HF0)
        house_to_hex.Add("K", &H30)
        house_to_hex.Add("L", &HB0)
        house_to_hex.Add("M", &H0)
        house_to_hex.Add("N", &H80)
        house_to_hex.Add("O", &H40)
        house_to_hex.Add("P", &HC0)

        device_to_hex.Add("1", &H6)
        device_to_hex.Add("2", &HE)
        device_to_hex.Add("3", &H2)
        device_to_hex.Add("4", &HA)
        device_to_hex.Add("5", &H1)
        device_to_hex.Add("6", &H9)
        device_to_hex.Add("7", &H5)
        device_to_hex.Add("8", &HD)
        device_to_hex.Add("9", &H7)
        device_to_hex.Add("10", &HF)
        device_to_hex.Add("11", &H3)
        device_to_hex.Add("12", &HB)
        device_to_hex.Add("13", &H0)
        device_to_hex.Add("14", &H8)
        device_to_hex.Add("15", &H4)
        device_to_hex.Add("16", &HC)

        com_to_hex.Add("ALL_UNITS_OFF", &H0)
        com_to_hex.Add("ALL_LIGHTS_ON", &H1)
        com_to_hex.Add("ON", &H2)
        com_to_hex.Add("OFF", &H3)
        com_to_hex.Add("DIM", &H4)
        com_to_hex.Add("BRIGHT", &H5)
        com_to_hex.Add("ALL_LIGHTS_OFF", &H6)
        com_to_hex.Add("EXTENDED_CODE", &H7)
        com_to_hex.Add("HAIL_REQ", &H8)
        com_to_hex.Add("HAIL_ACK", &H9)
        com_to_hex.Add("PRESET_DIM_1", &HA)
        com_to_hex.Add("PRESET_DIM_2", &HB)
        com_to_hex.Add("EXTENDED_DATA_TRANSFER", &HC)
        com_to_hex.Add("STATUS_ON", &HD)
        com_to_hex.Add("STATUS_OFF", &HE)
        com_to_hex.Add("STATUS_REQUEST", &HF)
    End Sub

    Public Function ouvrir(ByVal numero As String) As String
        Try
            If Not port_ouvert Then
                port_name = numero 'pour se rapeller du nom du port
                port.PortName = port_name 'nom du port : COM1
                port.BaudRate = 4800 'vitesse du port 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200
                port.Parity = IO.Ports.Parity.None 'pas de parité
                port.StopBits = IO.Ports.StopBits.One 'un bit d'arrêt par octet
                port.DataBits = 8 'nombre de bit par octet
                'port.Encoding = System.Text.Encoding.GetEncoding(1252)  'Extended ASCII (8-bits)
                'port.ReadBufferSize = CInt(4096)
                'port.ReceivedBytesThreshold = 1
                port.StopBits = StopBits.One
                port.Handshake = IO.Ports.Handshake.XOnXOff
                port.ReadTimeout = 500
                port.WriteTimeout = 500
                port.Open()
                port_ouvert = True
                'If port.IsOpen Then
                '    port.DtrEnable = True
                '    port.RtsEnable = True
                '    port.DiscardInBuffer()
                'End If
                Return ("Port " & port_name & " ouvert")
            Else
                Return ("ERR: Port " & port_name & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
                port_ouvert = False
                'suppression de l'attente de données à lire
                RemoveHandler port.DataReceived, AddressOf DataReceived
                'fermeture des ports
                If (Not (port Is Nothing)) Then ' The COM port exists.
                    If port.IsOpen Then
                        Dim limite As Integer = 0
                        Do While (port.BytesToWrite > 0 And limite < 100) ' Wait for the transmit buffer to empty.
                            limite = limite + 1
                        Loop
                        limite = 0
                        Do While (port.BytesToRead > 0 And limite < 100) ' Wait for the receipt buffer to empty.
                            limite = limite + 1
                        Loop
                        port.Close()
                        port.Dispose()
                        Return ("Port " & port_name & " fermé")
                    End If
                    Return ("Port " & port_name & "  est déjà fermé")
                End If
                Return ("ERR: Port " & port_name & " n'existe pas")
            End If
        Catch ex As UnauthorizedAccessException
            Return ("ERR: Port " & port_name & " IGNORE") ' The port may have been removed. Ignore.
        End Try
        Return True
    End Function

    Public Function lancer() As String
        AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)
        Return "Handler COM OK"
    End Function

    ''' <summary>
    ''' Traite les infos reçus
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
        Try
            Dim count As Integer = 0
            count = port.BytesToRead
            If count > 0 And port_ouvert Then
                port.Read(BufferIn, 0, count)
                Select Case BufferIn(0)
                    Case INTERFACE_CQ
                        'L'interface demande au pc de lui envoyer des données
                        port.Write(COMPUTER_READY)

                        '' Attend le reste des données
                        Dim Time_Out As Integer = 0
                        Do While Time_Out <= 20 And port.BytesToRead = 0
                            System.Threading.Thread.Sleep(500)
                            Time_Out += 1
                        Loop

                        ''A t-on reçu des données?
                        If Time_Out >= 20 Then
                            'GetData = ("Get Data,2: Temps d'attente dépassé")
                            Exit Sub
                        End If

                        port.Read(BufferIn, 0, count)
                        TraiteLire(BufferIn)
                    Case CM11_CLOCK_REQ
                        ' Power failure macro refresh request (Chr165 = 0xA5) Erreur CM11
                        ' Power fail/recovery detected.
                        port.Write(ACK_DEF_CM11) '0xfb
                    Case CP10_CLOCK_REQ

                    Case Else
                End Select
            End If
        Catch Ex As Exception
            WriteLog("ERR: X10 Datareceived : " & Ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Traite la trame reçu provenant d'un device
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <remarks></remarks>
    Private Sub TraiteLire(ByVal Data() As Byte)
        Dim BufSize As Integer = 0
        Dim Recieved_HouseCode As String
        Dim Recieved_DeviceCode As String
        Dim Recieved_Function As String
        Dim Recieved_FAMask As String
        Dim Recieved_Variation As Integer
        Dim FlagOk As Boolean = False

        'Wake-up and data recieved

        Dim trame() As Byte = Data
        BufSize = CInt(trame(0)) 'récupère la taille de la trame qui ne peu faire que 10 octets maxi

        'Byte   Function
        '0      Upload Buffer Size
        '1      Function / Address Mask
        '2      Data Byte #0
        '3      Data Byte #1
        '4      Data Byte #2
        '5      Data Byte #3
        '6      Data Byte #4
        '7      Data Byte #5
        '8      Data Byte #6
        '9      Data Byte #7

        If BufSize <= 10 Then 'Vérifie qu'il ne doit y avoir que 10 octet maximum qui doivent être envoyé sinon message d'erreur

            ' Le mask représente les octets 2 à 9 (bit0 pour octet2, bit1 pour octet3..,bit 8 pour octet9)
            ' Si le bit est à 0 cela veut dire que l'octet correspondant est une Adresse et si le bit est à 1 c'est une fonction
            Recieved_FAMask = Int2Bin(CInt(trame(1)))
            Recieved_FAMask = StrReverse(Recieved_FAMask)

            For i = 2 To BufSize
                Recieved_HouseCode = GetHouse(Mid(trame(i), 1, 4))

                Select Case Mid(Recieved_FAMask, (i - 1), 1)
                    Case "0" 'Le Mask est à 0 donc c'est une adresse
                        Recieved_DeviceCode = GetDevice(Mid(trame(i), 5, 4))

                    Case "1" 'Le Mask est à 1 donc c'est une fonction
                        Recieved_Function = GetFunction(Mid(trame(i), 5, 4))
                        If Recieved_Function = "5" Or Recieved_Function = "6" Then
                            'C'est une fonction Dim ou Bright donc octet suivant c'est la valeur de variation
                            Recieved_Variation = CInt(trame(i + 1))
                            i += 1
                        End If
                    Case Else 'C'est une erreur car ni adresse ni fonction

                End Select
            Next
        Else
            'ERREUR TROP DE BYTES A RECEVOIR MAX 10
        End If

    End Sub

    Public Function ecrire(ByVal adresse As String, ByVal commande As String, ByVal data As Integer) As String
        'adresse= adresse du composant : A1
        'commande : ON, OFF...
        'data

        Dim axbData(5) As Byte
        Dim ReadaxbData(1) As Byte
        Dim xbCheckSum As Byte
        Dim nbboucle As Integer

        If port_ouvert Then
            If Not OutPortDevice Then
                OutPortDevice = True

                'suppression de l'attente de données à lire
                RemoveHandler port.DataReceived, AddressOf DataReceived

                Try
                    'composition des messages à envoyer
                    axbData(0) = STANDARD_ADDRESS
                    axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or device_to_hex(Microsoft.VisualBasic.Right(adresse, adresse.Length - 1))
                    axbData(2) = ACK
                    axbData(3) = ((data * 8) And 255) Or STANDARD_FUNCTION
                    axbData(4) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or com_to_hex(commande)
                Catch ex As Exception
                    OutPortDevice = False
                    Return ("ERR: X10: messages non valides : " & adresse & "-" & commande & " --> " & ex.Message)
                End Try

                Try

                    Dim donnee As Byte() = {axbData(0), axbData(1)}
                    nbboucle = 0
                    Do
                        'ecriture de H4 et housecode-devicecode
                        port.Write(donnee, 0, 2)
                        System.Threading.Thread.Sleep(50)

                        'lecture du checksum renvoyé par le module
                        ReadaxbData(0) = port.ReadByte()
                        xbCheckSum = (axbData(0) + axbData(1)) And &HFF
                        nbboucle += 1
                    Loop Until ReadaxbData(0) = xbCheckSum Or nbboucle >= 4

                    'le chesksum n'a jamais été bon
                    If nbboucle >= 4 Then
                        OutPortDevice = False
                        Return ("ERR: X10 : cheksum non valide")
                    End If

                    'on envoie le ack
                    Dim donnee2 As Byte() = {axbData(2)}
                    port.Write(donnee2, 0, 1)

                    'on lit la reponse Interface ready
                    'System.Threading.Thread.Sleep(500)
                    'ReadaxbData(0) = port.ReadByte()
                    'If (ReadaxbData(0) <> INFERFACE_READY) Then
                    '    OutPortDevice = False
                    '    Return ("ERR: X10: INTERFACE NOT READY")
                    'End If

                    Dim donnee3 As Byte() = {axbData(3), axbData(4)}
                    nbboucle = 0
                    Do
                        'ecriture de la H6 + valeur du DIM et housecode-commandecode
                        port.Write(donnee3, 0, 2)
                        System.Threading.Thread.Sleep(50)

                        'lecture du checksum renvoyé par le module
                        ReadaxbData(0) = port.ReadByte()
                        xbCheckSum = (axbData(3) + axbData(4)) And &HFF
                        nbboucle += 1
                    Loop Until ReadaxbData(0) = xbCheckSum Or nbboucle >= 4

                    'le chesksum n'a jamais été bon
                    If nbboucle >= 4 Then
                        OutPortDevice = False
                        Return ("ERR: X10 : cheksum non valide")
                    End If

                    'on envoie le ack
                    port.Write(donnee2, 0, 1)

                    'on lit la reponse Interface ready
                    'System.Threading.Thread.Sleep(500)
                    'ReadaxbData(0) = port.ReadByte()
                    'If (ReadaxbData(0) <> INFERFACE_READY) Then
                    '    OutPortDevice = False
                    '    Return ("ERR: X10: INTERFACE NOT READY")
                    'End If

                Catch ex As Exception
                    Return ("ERR: X10: " & ex.Message)
                End Try
            Else
                OutPortDevice = False
                Return "ERR: X10: ecriture déjà en cours"
            End If

            AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)

            OutPortDevice = False
            Return "X10: OK"
        Else
            Return "ERR: X10: Port fermé"
        End If

    End Function

    ''' <summary>
    ''' Retourne la date et l'heure, la version
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function X10_GetClock() As String

        Dim X10_Recieved_Trame As String
        Dim X10_Send_Command As String
        Dim Trame As String 'Trame envoyé depuis le CM11 en binaire
        Dim Junk As String
        Dim j As Integer
        Dim tablS(14) As String
        Dim GClk_DimStatDev As String 'Dim status of the monitored devices
        Dim GClk_StatDev As String ''On/off status of the monitored devices
        Dim GClk_AdrDev As String ''currently addressed monitored devices
        Dim GClk_FirmwareRev As String 'Firmware revision level 0 to 15
        Dim GClk_MonitHC As String 'Monitored house code
        Dim GClk_Day As String 'Day Mask  (SMTWTFS, bit 57=Sunday)
        Dim GClk_Date As String 'Current year day
        Dim GClk_Hour As String ''Current time (hours/2, ranging from 0 to 11)
        Dim GClk_Minute As String 'Current time (minutes, ranging from 0 to 119)
        Dim GClk_Second As String 'Current time (seconds)
        Dim GClK_Battery As String 'Battery Timer (set to 0xffff on reset)
        Dim SupMinut As Boolean

        ' Prepare the Command and function
        X10_Send_Command = "10001011" '0x8b=> demander l'heure
        X10_Send_Command = Bin2Int(X10_Send_Command) 'Convert to an Integer
        X10_Send_Command = Chr(X10_Send_Command) 'Convert to ASCII binary value

        ' Send the command to the CM11
        port.Write(X10_Send_Command)

        ' Wait for the checksum byte to be returned
        Dim Time_Out As Integer = 0
        Do While Time_Out <= 2000 And port.BytesToRead = 0
            System.Threading.Thread.Sleep(100)
            Time_Out += 1
        Loop
        ' Do we have data?
        If Time_Out >= 2000 Then
            X10_GetClock = ("X10_GetClock,1: time out dépassé - Annulé")
            Exit Function
        End If

        If (port.BytesToRead * 8) <> 112 Then 'Vérifie que ce qui est reçu correspond bien au nombre de bits attendus
            X10_GetClock = ("X10_GetClock,2: Mauvaise réception - Aborted")
            Exit Function
        End If

        ' Reçoit la trame reçue
        X10_Recieved_Trame = port.ReadLine

        'Découpe et converti la trame reçu par octet
        For j = 1 To Len(X10_Recieved_Trame)
            tablS(j) = Format(Int2Bin(Asc(Mid(X10_Recieved_Trame, j, 1))), "00000000")
            If tablS(j) = "" Then
                tablS(j) = "00000000"
            End If
            Trame = Trame & tablS(j)
        Next j


        GClk_DimStatDev = Mid(Trame, 97, 16) 'Dim status of the monitored devices
        GClk_StatDev = Mid(Trame, 81, 16) 'On/off status of the monitored devices
        GClk_AdrDev = Mid(Trame, 65, 16) 'currently addressed monitored devices
        GClk_FirmwareRev = Mid(Trame, 61, 4)  'Firmware revision level 0 to 15
        GClk_MonitHC = Mid(Trame, 57, 4)  'Monitored house code
        GClk_Day = Mid(Trame, 50, 7)  'Day Mask  (SMTWTFS, bit 57=Sunday)
        GClk_Date = Mid(Trame, 41, 9)  'Current year day
        GClk_Hour = Mid(Trame, 33, 8)   'Current time (hours/2, ranging from 0 to 11)
        GClk_Minute = Mid(Trame, 25, 8)  'Current time (minutes, ranging from 0 to 119)
        GClk_Second = Mid(Trame, 17, 8) 'Current time (seconds)
        GClK_Battery = Mid(Trame, 1, 16) 'Battery Timer (set to 0xffff on reset)

        'Retourne la version firmware du CM11 *****************
        GClk_FirmwareRev = Bin2Int(GClk_FirmwareRev)
        If GClk_FirmwareRev <> 8 Then 'N° firmware différent recommencer si err com ou autre
            X10_GetClock = ("X10_GetClock,3: Numéro du firmware différent - Aborted")
            Exit Function
        End If

        'Retourne le jour *************************************
        GClk_Day = Format(GClk_Day, "0000000")
        GClk_Day = Bin2Day(GClk_Day)

        'Retourne les secondes ************************************
        GClk_Second = Format(Bin2Int(GClk_Second), "00")

        'retourne les minutes (sachant que minutes va de 0 à 120 donc si supérieur à 59 rajouter 1 à heure
        GClk_Minute = Bin2Int(GClk_Minute)
        If CInt(GClk_Minute) > 59 Then
            GClk_Minute = GClk_Minute - 60
            SupMinut = True
        Else
            SupMinut = False
        End If
        GClk_Minute = Format(GClk_Minute, "00")

        'retourne les heures (sachant heure est /2) ****************
        GClk_Hour = Bin2Int(GClk_Hour)
        GClk_Hour = GClk_Hour * 2
        If SupMinut = True Then GClk_Hour += 1

        'Date (Nombre jour de l'année (de 0 pour 1 janvier à 365 pour 31 décembre)*****************************************************
        If Bin2Int(GClk_Date) > 365 Then
            X10_GetClock = "Get Clock,4 - Il y a plus de 365 jours donc problème"
            Exit Function
        End If

        If Bin2dbl(GClk_Date) > 365 Then Exit Function
        GClk_Date = Jour2Date(Bin2Int(GClk_Date))

        'Battery timer ********************************************
        GClK_Battery = Bin2dbl(GClK_Battery)

        X10_GetClock = "Battery: " & GClK_Battery & " - House: " & GetHouse(GClk_MonitHC) & " Add: " & GetAdd(GClk_AdrDev) & " - firmware: " & GClk_FirmwareRev & " - " & GClk_Day & " " & GClk_Date & " " & GClk_Hour & ":" & GClk_Minute & ":" & GClk_Second
    End Function

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        'If STRGS.InStr(message, "ERR:") > 0 Then
        '    domos_svc.log("RFX : " & message, 2)
        'Else
        '    domos_svc.log("RFX : " & message, 9)
        'End If
    End Sub

#Region "Conversion"
    ''' <summary>
    ''' Renvoi la fonction depuis la valeur binaire
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetFunction(ByVal Bin As String) As String
        Bin = Format(Bin, "0000")
        Select Case Bin
            Case "0000" : GetFunction = "1"     'All units off
            Case "0001" : GetFunction = "2"     'All lights on
            Case "0010" : GetFunction = "3"     'On
            Case "0011" : GetFunction = "4"     'Off
            Case "0100" : GetFunction = "5"     'Dim
            Case "0101" : GetFunction = "6"     'Bright
            Case "0110" : GetFunction = "7"     'All lights off
            Case "0111" : GetFunction = "8"     'Extended code
            Case "1000" : GetFunction = "9"     'Hail request
            Case "1001" : GetFunction = "10"    'Hail Ack
            Case "1010" : GetFunction = "11"    'Preset dim 1
            Case "1011" : GetFunction = "12"    'Preset dim 2
            Case "1100" : GetFunction = "13"    'Extended data transfer
            Case "1101" : GetFunction = "14"    'Status on
            Case "1110" : GetFunction = "15"    'Status off
            Case "1111" : GetFunction = "16"    'Status request
        End Select
    End Function

    ''' <summary>
    ''' Renvoi le numéro d'adresse depuis la valeur Binaire
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDevice(ByVal Bin As String) As String
        Bin = Format(Bin, "0000")
        Select Case Bin
            Case "0110" : GetDevice = "1"
            Case "1110" : GetDevice = "2"
            Case "0010" : GetDevice = "3"
            Case "1010" : GetDevice = "4"
            Case "0001" : GetDevice = "5"
            Case "1001" : GetDevice = "6"
            Case "0101" : GetDevice = "7"
            Case "1101" : GetDevice = "8"
            Case "0111" : GetDevice = "9"
            Case "1111" : GetDevice = "10"
            Case "0011" : GetDevice = "11"
            Case "1011" : GetDevice = "12"
            Case "0000" : GetDevice = "13"
            Case "1000" : GetDevice = "14"
            Case "0100" : GetDevice = "15"
            Case "1100" : GetDevice = "16"
        End Select
    End Function

    ''' <summary>
    ''' Renvoi le Code House depuis la valeur Binaire
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetHouse(ByVal Bin As String) As String
        Bin = Format(Bin, "0000")
        Select Case Bin
            Case "0110" : GetHouse = "A"
            Case "1110" : GetHouse = "B"
            Case "0010" : GetHouse = "C"
            Case "1010" : GetHouse = "D"
            Case "0001" : GetHouse = "E"
            Case "1001" : GetHouse = "F"
            Case "0101" : GetHouse = "G"
            Case "1101" : GetHouse = "H"
            Case "0111" : GetHouse = "I"
            Case "1111" : GetHouse = "J"
            Case "0011" : GetHouse = "K"
            Case "1011" : GetHouse = "L"
            Case "0000" : GetHouse = "M"
            Case "1000" : GetHouse = "N"
            Case "0100" : GetHouse = "O"
            Case "1100" : GetHouse = "P"
        End Select
    End Function

    ''' <summary>
    ''' Conversion Entier en Binaire 
    ''' </summary>
    ''' <param name="Dec"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Int2Bin(ByVal Dec As Integer) As String
        Dim tdec As Integer = Dec
        Dim Bin As String = ""
        Do While tdec > 0
            If tdec / 2 = tdec \ 2 Then
                Bin = "0" + Bin
            ElseIf tdec / 2 <> tdec \ 2 Then
                Bin = "1" + Bin
            End If
            tdec = tdec \ 2
        Loop
        Return Format(Bin, "00000000")
    End Function

    ''' <summary>
    ''' Conversion Binaire en Entier 
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function Bin2Int(ByVal Bin As String) As Integer
        Dim ax As Integer
        Dim z As Integer
        For y As Integer = Len(Bin) To 1 Step -1
            If Mid(Bin, y, 1) = "1" Then ax = ax + (2 ^ z)
            z += 1
        Next
        Bin2Int = ax
    End Function

    ''' <summary>
    ''' Conversion Bin en jour
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function Bin2Day(ByVal Bin As String) As String
        'à modifier car mapage incorrect voir le retour d'état de l'interface

        Select Case Bin
            Case "0000001" : Bin2Day = "Dimanche"
            Case "0000010" : Bin2Day = "Lundi"
            Case "0000100" : Bin2Day = "Mardi"
            Case "0001000" : Bin2Day = "Mercredi"
            Case "0010000" : Bin2Day = "Jeudi"
            Case "0100000" : Bin2Day = "Vendredi"
            Case "1000000" : Bin2Day = "Samedi"
            Case Else : Bin2Day = "?"
        End Select
    End Function

    ''' <summary>
    ''' Conversion Binaire en Double
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Bin2dbl(ByVal Bin As String) As Double
        Dim y As Integer
        Dim z As Integer
        Dim ax As Double
        For y = Len(Bin) To 1 Step -1
            If Mid(Bin, y, 1) = "1" Then ax = ax + (2 ^ z)
            z += 1
        Next
        Bin2dbl = ax
    End Function

    ''' <summary>
    ''' Convertie le nombre de jour en date
    ''' </summary>
    ''' <param name="NbJour"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Jour2Date(ByVal NbJour As Integer) As String
        Dim Cal As DateTime = CDate("01/01/" & Now.Year)
        Cal.AddDays(NbJour)
        Jour2Date = Cal
    End Function

    ''' <summary>
    ''' Renvoi l'adresse pour getclock
    ''' </summary>
    ''' <param name="Bin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetAdd(ByVal Bin As String) As String
        Select Case Bin
            Case "1000000000000000" : GetAdd = "9"     '
            Case "0100000000000000" : GetAdd = "1"     '
            Case "0010000000000000" : GetAdd = "7"     '
            Case "0001000000000000" : GetAdd = "15"     '
            Case "0000100000000000" : GetAdd = "11"     '
            Case "0000010000000000" : GetAdd = "3"     '
            Case "0000001000000000" : GetAdd = "5"     '
            Case "0000000100000000" : GetAdd = "13"     '
            Case "0000000010000000" : GetAdd = "10"     '
            Case "0000000001000000" : GetAdd = "2"    '
            Case "0000000000100000" : GetAdd = "8"    '
            Case "0000000000010000" : GetAdd = "16"    '
            Case "0000000000001000" : GetAdd = "12"    '
            Case "0000000000000100" : GetAdd = "4"    '
            Case "0000000000000010" : GetAdd = "6"    '
            Case "0000000000000001" : GetAdd = "14"    '
            Case Else : GetAdd = "0"
        End Select
    End Function
#End Region
End Class