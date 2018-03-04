Imports System.IO
Imports System.Threading
Imports DSharpPlus
Imports MySql.Data.MySqlClient

Public Class Form1
    Private MySQLString = String.Empty
    Private WithEvents DiscordClient As DiscordClient
    Private ChannelId As String = String.Empty
    Private ServerName As String = String.Empty
    Private Language As String = String.Empty

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim ConfigFile = New StreamReader("Config.txt")
        Dim currentline As String = String.Empty
        Dim MySQLServer As String = String.Empty
        Dim MySQLUser As String = String.Empty
        Dim MySQLPassword As String = String.Empty
        Dim MySQLDatabase As String = String.Empty
        Dim token As String = String.Empty
        While ConfigFile.EndOfStream = False
            currentline = ConfigFile.ReadLine
            If currentline.Contains("mysqlserver=") Then
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
            ElseIf currentline.Contains("servername") Then
                Dim GetServerName As String() = currentline.Split("=")
                ServerName = GetServerName(1)
                Me.Text = "Posts to " + ServerName + " Discord Server"
            ElseIf currentline.Contains("token") Then
                Dim GetToken As String() = currentline.Split("=")
                token = GetToken(1)
            ElseIf currentline.Contains("channelid") Then
                Dim GetChannel As String() = currentline.Split("=")
                ChannelId = GetChannel(1)
            ElseIf currentline.Contains("language") Then
                Dim GetLanguage As String() = currentline.Split("=")
                Language = GetLanguage(1)
            End If
        End While
        MySQLString = "server=" + MySQLServer + ";user=" + MySQLUser + ";database=" + MySQLDatabase + ";port=3306;password=" + MySQLPassword + ";"
        Dim dcfg As New DiscordConfig
        With dcfg
            .Token = token
            .TokenType = TokenType.Bot
            .LogLevel = LogLevel.Debug
            .AutoReconnect = True
        End With
        Me.DiscordClient = New DiscordClient(dcfg)
        Await Me.DiscordClient.ConnectAsync()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim Thread1 As New Thread(Sub() PostToDiscord())
        Thread1.Start()
        Button1.Text = "Running"
    End Sub

    Public Async Sub PostToDiscord()
        Dim query As String = "SELECT * FROM newposts WHERE posted=0 AND channel='" & ServerName & "'"
        While True
            Try
                Dim Connection = New MySqlConnection(MySQLString)
                Dim Command As New MySqlCommand(query, Connection)
                Connection.Open()
                Dim reader As MySqlDataReader = Command.ExecuteReader
                If reader.HasRows Then
                    While reader.Read
                        Await SendPost(reader("username"), reader("link"))
                        Dim SQLQuery2 = "UPDATE newposts SET posted=1 WHERE id = " & reader("id") & ";INSERT INTO newpostsprocessed (username, link, posted, channel) SELECT username, link, posted, channel FROM newposts WHERE posted=1;DELETE FROM newposts WHERE posted=1;"
                        Dim Connection2 = New MySqlConnection(MySQLString)
                        Dim Command2 As New MySqlCommand(SQLQuery2, Connection2)
                        Connection2.Open()
                        Command2.ExecuteNonQuery()
                        Connection2.Close()
                        Thread.Sleep(2000)
                    End While
                End If
                Connection.Close()
                Thread.Sleep(2000)
            Catch ex As Exception
                Continue While
            End Try
        End While
    End Sub

    Private Async Function SendPost(username As String, link As String) As Task
        Dim Message As String = String.Empty
        If Language = "en" Then
            Message = "New post from @" & username & ". Post Link: " & link
        Else
            Message = "Nuevo post de @" & username & ". Link del post: " & link
        End If
        Await DiscordClient.SendMessageAsync(Await DiscordClient.GetChannelAsync(Convert.ToUInt64(ChannelId)), Message)
    End Function
End Class
