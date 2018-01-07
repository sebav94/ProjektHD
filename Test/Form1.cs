using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ProjektHD
{
	public partial class Form1 : Form
	{
		StringBuilder op = new StringBuilder(); //opinia
		StringBuilder wd = new StringBuilder(); //wady
		StringBuilder zl = new StringBuilder(); //zalety
		StringBuilder gw = new StringBuilder(); //gwiazdki
		StringBuilder dt = new StringBuilder(); //data
		StringBuilder pln = new StringBuilder(); //polecenie
		StringBuilder pz = new StringBuilder(); //przydatna
		StringBuilder pn = new StringBuilder(); //nie przydatna
		private string[] opinieL;
		private string[] gwiazdkiL;
		private string[] lajkiL;
		private string[] nielajkiL;
		private string[] datyL;
		private string[] poleceniaL;

		public Form1()
		{
			InitializeComponent();
		}

		public void StworzPlikCSV(DataTable dt, string sciezka)
		{
			StreamWriter sw = new StreamWriter(sciezka, false);
			//naglowki
			int j = dt.Columns.Count;
			for (int i = 0; i < j; i++)
			{
				sw.Write(dt.Columns[i]);
				if (i < i - 1)
				{
					sw.Write(";", "");
				}
			}
			sw.Write(sw.NewLine);

			// dane
			foreach (DataRow dr in dt.Rows)
			{
				for (int i = 0; i < j; i++)
				{
					if (!Convert.IsDBNull(dr[i]))
					{
						sw.Write(dr[i].ToString());
					}
					if (i < j - 1)
					{
						sw.Write(";", "");
					}
				}
				sw.Write(sw.NewLine);
			}
			sw.Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				MessageBox.Show("Połączono poprawnie!");
				button1.Enabled = true;
				button3.Enabled = true;
				button6.Enabled = true;
				button7.Enabled = true;
				button8.Enabled = true;
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Brak połączenia sprawdź dane!");
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			richTextBox1.Text = null;
			WebClient client = new WebClient();
			Byte[] pageData = client.DownloadData("https://www.ceneo.pl/" + textBox1.Text + "#tab=reviews");
			string pageHtml = Encoding.UTF8.GetString(pageData);
			string txt = pageHtml;
			foreach (Match match in Regex.Matches(txt, "<p class=\"product-review-body\">(.*)</p>".ToString()))
			{
				op.Append(match.Groups[1].Value + "\n");
			}
			//foreach (Match match in Regex.Matches(txt, "<div class=\"pros-cell\">(.*)</div>".ToString()))
			//{
			//	wd.Append(match.Groups[1].Value + "\n");
			//}
			//foreach (Match match in Regex.Matches(txt, "<p class=\"product-review-body\">(.*)</p>".ToString()))
			//{
			//	zl.Append(match.Groups[1].Value + "\n");
			//}
			foreach (Match match in Regex.Matches(txt, "<span class=\"review-score-count\">(.*)</span>".ToString()))
			{
				gw.Append(match.Groups[1].Value.Replace("/", "./") + "\n");
			}
			//foreach (Match match in Regex.Matches(txt, "<div class=\"reviewer-name-line\">(.*)</div>".ToString()))
			//{
			//	at.Append(match.Groups[1].Value + "\n");
			//}
			foreach (Match match in Regex.Matches(txt, "<time datetime=\"(.*)\">".ToString()))
			{
				dt.Append(match.Groups[1].Value.Remove(10, 9) + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<em class=\"product-recommended\">(.*)</em>".ToString()))
			{
				pln.Append(match.Groups[1].Value + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<span id=\"votes-yes-[0-9]+\">(.*)</span>".ToString()))
			{
				pz.Append(match.Groups[1].Value + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<span id=\"votes-no-[0-9]+\">(.*)</span>".ToString()))
			{
				pn.Append(match.Groups[1].Value + "\n");
			}
			richTextBox1.Text = op.ToString() + wd.ToString() + gw.ToString() + pln.ToString() + pz.ToString() + pn.ToString() + dt.ToString();
			//transformacja
			string[] opinie = op.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] polecenia = pln.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] gwiazdki = gw.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] daty = dt.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] lajki = pz.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] nielajki = pn.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			//ładowanie danych do bazy
			MySqlDataAdapter mySqlDataAdapter;
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				MySqlCommand cmd = new MySqlCommand();
				MySqlCommand spr = new MySqlCommand();
				cmd.Connection = cnn;
				spr.Connection = cnn;
				int i = 0;
				Random x = new Random();
				foreach (var polecenie in polecenia)
				{
					spr.CommandText = "SELECT COUNT(opinia) FROM opinie WHERE opinia ='" + opinie[i] + "'";
					
					int sprOk = Convert.ToInt32(spr.ExecuteScalar());
					if(sprOk > 0)
					{
						//nic nie rób
					}
					else
					{
						cmd.CommandText = "INSERT INTO opinie (id, idCeneo, opinia, polecenie, gwiazdki, likes, dislikes, data) VALUES (" + x.Next(0, 99999999) + ",'" + textBox1.Text + "','" + opinie[i] + "','" + polecenie + "','" + gwiazdki[i] + "','" + lajki[i] + "','" + nielajki[i] + "','" + daty[i] + "')";
						cmd.ExecuteNonQuery();
					}
					i++;
				}
				mySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM opinie", cnn);
				DataSet ds = new DataSet();
				mySqlDataAdapter.Fill(ds);
				dataGridView1.DataSource = ds.Tables[0];
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Wystąpił błąd, spróbuj ponownie!");
			}
			opinie = null;
			polecenia = null;
			gwiazdki = null;
			daty = null;
			lajki = null;
			nielajki = null;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			richTextBox1.Text = null;
			WebClient client = new WebClient();
			Byte[] pageData = client.DownloadData("https://www.ceneo.pl/" + textBox1.Text + "#tab=reviews");
			string pageHtml = Encoding.UTF8.GetString(pageData);
			string txt = pageHtml;
			foreach (Match match in Regex.Matches(txt, "<p class=\"product-review-body\">(.*)</p>".ToString()))
			{
				op.Append(match.Groups[1].Value + "\n");
			}
			//foreach (Match match in Regex.Matches(txt, "<div class=\"pros-cell\">(.*)</div>".ToString()))
			//{
			//	wd.Append(match.Groups[1].Value + "\n");
			//}
			//foreach (Match match in Regex.Matches(txt, "<p class=\"product-review-body\">(.*)</p>".ToString()))
			//{
			//	zl.Append(match.Groups[1].Value + "\n");
			//}
			foreach (Match match in Regex.Matches(txt, "<span class=\"review-score-count\">(.*)</span>".ToString()))
			{
				gw.Append(match.Groups[1].Value + "\n");
			}
			//foreach (Match match in Regex.Matches(txt, "<div class=\"reviewer-name-line\">(.*)</div>".ToString()))
			//{
			//	at.Append(match.Groups[1].Value + "\n");
			//}
			foreach (Match match in Regex.Matches(txt, "<time datetime=\"(.*)\">".ToString()))
			{
				dt.Append(match.Groups[1].Value.Remove(10, 9) + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<em class=\"product-recommended\">(.*)</em>".ToString()))
			{
				pln.Append(match.Groups[1].Value + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<span id=\"votes-yes-[0-9]+\">(.*)</span>".ToString()))
			{
				pz.Append(match.Groups[1].Value + "\n");
			}
			foreach (Match match in Regex.Matches(txt, "<span id=\"votes-no-[0-9]+\">(.*)</span>".ToString()))
			{
				pn.Append(match.Groups[1].Value + "\n");
			}
			richTextBox1.Text = op.ToString() + wd.ToString() + gw.ToString() + pln.ToString() + pz.ToString() + pn.ToString() + dt.ToString();

			button4.Visible = true;
			button3.Visible = false;
		}

		private void button4_Click(object sender, EventArgs e)
		{
			//transformacja
			string[] opinie = op.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] polecenia = pln.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] gwiazdki = gw.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] daty = dt.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] lajki = pz.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			string[] nielajki = pn.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

			opinieL = opinie;
			poleceniaL = polecenia;
			gwiazdkiL = gwiazdki;
			datyL = daty;
			lajkiL = lajki;
			nielajkiL = nielajki;

			button5.Visible = true;
			button4.Visible = false;
		}

		private void button5_Click(object sender, EventArgs e)
		{
			MySqlDataAdapter mySqlDataAdapter;
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				MySqlCommand cmd = new MySqlCommand();
				MySqlCommand spr = new MySqlCommand();
				cmd.Connection = cnn;
				spr.Connection = cnn;
				int i = 0;
				Random x = new Random();
				foreach (var polecenie in poleceniaL)
				{
					spr.CommandText = "SELECT COUNT(opinia) FROM opinie WHERE opinia ='" + opinieL[i] + "'";

					int sprOk = Convert.ToInt32(spr.ExecuteScalar());
					if (sprOk > 0)
					{
						//nic nie rób
					}
					else
					{
						cmd.CommandText = "INSERT INTO opinie (id, idCeneo, opinia, polecenie, gwiazdki, likes, dislikes, data) VALUES (" + x.Next(0, 99999999) + ",'" + textBox1.Text + "','" + opinieL[i] + "','" + polecenie + "','" + gwiazdkiL[i] + "','" + lajkiL[i] + "','" + nielajkiL[i] + "','" + datyL[i] + "')";
						cmd.ExecuteNonQuery();
					}
					i++;
				}
				mySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM opinie", cnn);
				DataSet DS = new DataSet();
				mySqlDataAdapter.Fill(DS);
				dataGridView1.DataSource = DS.Tables[0];
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Wystąpił błąd, spróbuj ponownie!");
			}
			opinieL = null;
			poleceniaL = null;
			gwiazdkiL = null;
			datyL = null;
			lajkiL = null;
			nielajkiL = null;
			button3.Visible = true;
			button5.Visible = false;
		}

		private void button6_Click(object sender, EventArgs e)
		{
			MySqlDataAdapter mySqlDataAdapter;
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				MySqlCommand cmd = new MySqlCommand();
				cmd.Connection = cnn;
				cmd.CommandText = "DELETE FROM opinie";
				cmd.ExecuteNonQuery();
				MessageBox.Show("Baza danych wyczyszczona!");
				mySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM opinie", cnn);
				DataSet DS = new DataSet();
				mySqlDataAdapter.Fill(DS);
				dataGridView1.DataSource = DS.Tables[0];
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Wystąpił błąd, spróbuj ponownie!");
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			MySqlDataAdapter mySqlDataAdapter;
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				mySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM opinie WHERE idCeneo ='" + textBox6.Text + "'", cnn);
				DataSet DS = new DataSet();
				mySqlDataAdapter.Fill(DS);
				dataGridView1.DataSource = DS.Tables[0];
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Wystąpił błąd, spróbuj ponownie!");
			}
		}

		private void button8_Click(object sender, EventArgs e)
		{			
			MySqlDataAdapter mySqlDataAdapter;
			string connetionString = null;
			MySqlConnection cnn;
			connetionString = "server=" + textBox2.Text + ";database=" + textBox3.Text + ";uid=" + textBox4.Text + ";pwd=" + textBox5.Text + ";";
			cnn = new MySqlConnection(connetionString);
			try
			{
				cnn.Open();
				string folderPath = "C:\\CSV\\Export.csv";
				DataTable dat = new DataTable();
				mySqlDataAdapter = new MySqlDataAdapter("SELECT * FROM opinie", cnn);
				mySqlDataAdapter.Fill(dat);
				dataGridView1.DataSource = dat;
				StworzPlikCSV(dat, folderPath);
				MessageBox.Show("Sukces!");
				cnn.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Wystąpił błąd, spróbuj ponownie!");
			}

		}
	}
}