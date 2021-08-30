using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace interface_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string nomeArquivo = "arquivoPecas.json";
        private readonly Regex interios = new Regex("[^0-9]+");
        private readonly Regex decimais = new Regex("[^0-9,-]+");

        public MainWindow()
        {
            InitializeComponent();

            txtCodigoPeca.Focus();
            
            lstDados.ItemsSource = CarregaDados();
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            Peca peca = new Peca();

            if (string.IsNullOrEmpty(txtCodigoPeca.Text) || string.IsNullOrEmpty(txtDescricaoPeca.Text) || 
                string.IsNullOrEmpty(txtLadoA.Text) || string.IsNullOrEmpty(txtLadoB.Text))
            {
                MessageBox.Show("Todos os campo devem ser preenchidos");
                return;
            }

            try
            {
                peca.codigo = Convert.ToInt32(txtCodigoPeca.Text);
                peca.descricao = txtDescricaoPeca.Text;
                peca.ladoA = Convert.ToDouble(txtLadoA.Text);
                peca.ladoB = Convert.ToDouble(txtLadoB.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Erro ao salvar dados. Verifique os dados digitados. Erro: " + ex.Message);
                return;
            }

            DadosPeca dadosPeca = new DadosPeca();
            dadosPeca.codigoPeca = txtCodigoPeca.Text;
            RemovePeca(dadosPeca);

            JArray arrayDados = LeArquivoJson();

            if (arrayDados != null)
            {
                JObject novoRegistro = JObject.Parse(JsonSerializer.Serialize(peca));
                arrayDados.Add(novoRegistro);

                if (SalvaArquivoJson(arrayDados))
                {
                    txtCodigoPeca.Clear();
                    txtDescricaoPeca.Clear();
                    txtLadoA.Clear();
                    txtLadoB.Clear();

                    lstDados.ItemsSource = CarregaDados();
                }
            }
        }

        private List<DadosPeca> CarregaDados()
        {
            List<DadosPeca> dadosPecas = new List<DadosPeca>();

            string dadosArquivo = File.ReadAllText(nomeArquivo);

            JArray arrayDaddos = null;
            try
            {
                arrayDaddos = JArray.Parse(dadosArquivo);
            }
            catch (Exception)
            {
                arrayDaddos = JArray.Parse("[]");
            }

            foreach(JObject peca in arrayDaddos)
            {
                DadosPeca pecaList = new DadosPeca();
                pecaList.codigoPeca = peca["codigo"].ToString();
                pecaList.descricaoPeca = peca["descricao"].ToString();
                pecaList.dimensaoPeca = peca["ladoA"].ToString() + " X " + peca["ladoB"].ToString();

                dadosPecas.Add(pecaList);
            }

            dadosPecas.Sort(
                (registro, comparador) => registro.codigoPeca.CompareTo(comparador.codigoPeca)
                );

            return dadosPecas;
        }

        private void btnAlterar_Click(object sender, RoutedEventArgs e)
        {
            DadosPeca pecaSelecionada = lstDados.SelectedItem as DadosPeca;

            JArray dadosArquivoPecas = LeArquivoJson();

            foreach(JObject peca in dadosArquivoPecas)
            {
                if (peca["codigo"].ToString().Equals(pecaSelecionada.codigoPeca))
                {
                    txtCodigoPeca.Text = peca["codigo"].ToString();
                    txtDescricaoPeca.Text = peca["descricao"].ToString();
                    txtLadoA.Text = peca["ladoA"].ToString();
                    txtLadoB.Text = peca["ladoB"].ToString();
                    break;
                }
            }
        }

        private JArray LeArquivoJson()
        {
            JArray arrayJson = null;

            string arquivoPecas = "";

            try
            {
                arquivoPecas = File.ReadAllText(nomeArquivo);
            }
            catch (Exception)
            {
                MessageBox.Show("Arquivo para salvar dados não encontrado. Criar o arquivo antes de continuar. \n" +
                 "Criar arquivo com o nome \"arquivoPecas.json\" na mesma pasta do executável.");

                return null;
            }

            try
            {
                arrayJson = JArray.Parse(arquivoPecas);
            }
            catch (Exception)
            {
                arrayJson = JArray.Parse("[]");
            }

            return arrayJson;
        }

        private bool SalvaArquivoJson(JArray arrayDados)
        {
            try
            {
                File.WriteAllText(nomeArquivo, arrayDados.ToString());
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("Erro ao salvar arquivo de dados. Erro: " + e.Message);
                return false;
            }
        }

        private void btnExcluir_Click(object sender, RoutedEventArgs e)
        {
            var retorno = MessageBox.Show("Confirma a exclusão do registro?", "Exclusão", MessageBoxButton.YesNo);
            if (retorno.Equals(MessageBoxResult.Yes))
            {
                DadosPeca pecaSelecionada = lstDados.SelectedItem as DadosPeca;

                if (RemovePeca(pecaSelecionada))
                {
                    lstDados.ItemsSource = CarregaDados();
                    MessageBox.Show("Registro removido com sucesso.");
                }
                else
                {
                    lstDados.ItemsSource = CarregaDados();
                    MessageBox.Show("Falha ao remover registro. Tente novamente.");
                }
            }
        }

        private Boolean RemovePeca(DadosPeca pecaSelecionada)
        {
            JArray dadosArquivoPecas = LeArquivoJson();
            Boolean removeuRegistro = false;

            foreach (JObject peca in dadosArquivoPecas)
            {
                if (peca["codigo"].ToString().Equals(pecaSelecionada.codigoPeca))
                {
                    dadosArquivoPecas.Remove(peca);
                    removeuRegistro = true;
                    break;
                }
            }

            if (SalvaArquivoJson(dadosArquivoPecas) && removeuRegistro)
                return true;
            else
                return false;
        }

        private void txtCodigoPeca_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = interios.IsMatch(e.Text);
        }

        private void txtLadoA_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = decimais.IsMatch(e.Text);
            if (e.Text.Equals(",") && txtLadoA.Text.Contains(","))
                e.Handled = true;
        }

        private void txtLadoB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = decimais.IsMatch(e.Text);
            if (e.Text.Equals(",") && txtLadoB.Text.Contains(","))
                e.Handled = true;
        }

        private void btnLimparPesquisa_Click(object sender, RoutedEventArgs e)
        {
            lstDados.ItemsSource = CarregaDados();
        }

        private void btnPesquisarPor_Click(object sender, RoutedEventArgs e)
        {
            List<DadosPeca> dadosPecas = new List<DadosPeca>();

            JArray dadosArquivoPecas = LeArquivoJson();

            if (cbxPesquisarPor.SelectedIndex == 0)
            {
                foreach (JObject peca in dadosArquivoPecas)
                {
                    if (peca["codigo"].ToString().Equals(txtPesquisarPor.Text))
                    {
                        DadosPeca pecaList = new DadosPeca();
                        pecaList.codigoPeca = peca["codigo"].ToString();
                        pecaList.descricaoPeca = peca["descricao"].ToString();
                        pecaList.dimensaoPeca = peca["ladoA"].ToString() + " X " + peca["ladoB"].ToString();

                        dadosPecas.Add(pecaList);
                    }
                }

            }
            else if (cbxPesquisarPor.SelectedIndex == 1)
            {
                foreach (JObject peca in dadosArquivoPecas)
                {
                    if (peca["descricao"].ToString().ToLower().Contains(txtPesquisarPor.Text.ToLower()))
                    {
                        DadosPeca pecaList = new DadosPeca();
                        pecaList.codigoPeca = peca["codigo"].ToString();
                        pecaList.descricaoPeca = peca["descricao"].ToString();
                        pecaList.dimensaoPeca = peca["ladoA"].ToString() + " X " + peca["ladoB"].ToString();

                        dadosPecas.Add(pecaList);
                    }
                }
            }
            else if (cbxPesquisarPor.SelectedIndex == 2)
            {
                foreach (JObject peca in dadosArquivoPecas)
                {
                    if (peca["ladoA"].ToString().Replace(".", ",").Contains(txtPesquisarPor.Text) ||
                        peca["ladoB"].ToString().Replace(".", ",").Contains(txtPesquisarPor.Text))
                    {
                        DadosPeca pecaList = new DadosPeca();
                        pecaList.codigoPeca = peca["codigo"].ToString();
                        pecaList.descricaoPeca = peca["descricao"].ToString();
                        pecaList.dimensaoPeca = peca["ladoA"].ToString() + " X " + peca["ladoB"].ToString();

                        dadosPecas.Add(pecaList);
                    }
                }
            }
            else
                MessageBox.Show("Selecione a opção de pesquisa desejada.");

            if (dadosPecas.Count > 0)
            {
                dadosPecas.Sort(
                                (registro, comparador) => registro.codigoPeca.CompareTo(comparador.codigoPeca)
                               );
                lstDados.ItemsSource = dadosPecas;
            }
            else
            {
                lstDados.ItemsSource = dadosPecas;
                MessageBox.Show("Pesquisa não retornou dados.");
                lstDados.ItemsSource = CarregaDados();
            }
        }
    }
}
