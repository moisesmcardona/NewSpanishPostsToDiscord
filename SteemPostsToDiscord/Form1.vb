Imports System.IO
Imports DSharpPlus
Imports MySql.Data.MySqlClient

Public Class Form1
    Private MySQLString = ""
    Private WithEvents DiscordClient As DiscordClient
    Private DiscordChannelObject As DiscordChannel
    Private WithEvents DiscordClientLogger As DebugLogger
    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim ConfigFile As StreamReader = New StreamReader("Config.txt")
        Dim currentline As String = String.Empty
        Dim MySQLServer As String = String.Empty
        Dim MySQLUser As String = String.Empty
        Dim MySQLPassword As String = String.Empty
        Dim MySQLDatabase As String = String.Empty
        Dim token As String = String.Empty
        While ConfigFile.EndOfStream = False
            currentline = ConfigFile.ReadLine
            If currentline.Contains("server") Then
                Dim GetServer As String() = currentline.Split("=")
                MySQLServer = GetServer(1)
            ElseIf currentline.Contains("username") Then
                Dim GetUsername As String() = currentline.Split("=")
                MySQLUser = GetUsername(1)
            ElseIf currentline.Contains("password") Then
                Dim GetPassword As String() = currentline.Split("=")
                MySQLPassword = GetPassword(1)
            ElseIf currentline.Contains("database") Then
                Dim GetDatabase As String() = currentline.Split("=")
                MySQLDatabase = GetDatabase(1)
            End If
        End While
        MySQLString = "server=" + MySQLServer + ";user=" + MySQLUser + ";database=" + MySQLDatabase + ";port=3306;password=" + MySQLPassword + ";"
        Dim dcfg As New DiscordConfig
        With dcfg
            .Token = My.Computer.FileSystem.ReadAllText("token.txt")
            .TokenType = TokenType.Bot
            .LogLevel = LogLevel.Debug
            .AutoReconnect = True
        End With
        Me.DiscordClient = New DiscordClient(dcfg)
        Me.DiscordClientLogger = Me.DiscordClient.DebugLogger
        Await Me.DiscordClient.ConnectAsync()
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim Thread1 As New System.Threading.Thread(Sub() PostToDiscord())
        Thread1.Start()
        Button1.Text = "Running"
    End Sub
    Public Sub PostToDiscord()
        Dim SQLQuery3 As String = "SELECT  * FROM newspanishposts WHERE posted=0"
        While True
            Try
                Dim Connection3 As MySqlConnection = New MySqlConnection(MySQLString)
                Dim Command3 As New MySqlCommand(SQLQuery3, Connection3)
                Connection3.Open()
                Dim reader3 As MySqlDataReader = Command3.ExecuteReader
                If reader3.HasRows Then
                    While reader3.Read
                        SendPost(reader3("username"), reader3("link"))
                        Dim SQLQuery4 As String = "DELETE FROM newspanishposts WHERE link='" & reader3("link") & "'"
                        Dim Connection4 As MySqlConnection = New MySqlConnection(MySQLString)
                        Dim Command4 As New MySqlCommand(SQLQuery4, Connection4)
                        Connection4.Open()
                        Command4.ExecuteNonQuery()
                        Connection4.Close()
                        Threading.Thread.Sleep(2000)
                    End While
                End If
                Connection3.Close()
                Threading.Thread.Sleep(2000)
            Catch ex As Exception
            End Try
        End While
    End Sub
    Private Async Sub SendPost(username As String, link As String)
        Dim Channel As DiscordChannel = Await DiscordClient.GetChannelAsync(368568796216295434)
        Await DiscordClient.SendMessageAsync(Channel, "Nuevo post de @" & username & ". Link del post: " & link)
    End Sub
End Class
