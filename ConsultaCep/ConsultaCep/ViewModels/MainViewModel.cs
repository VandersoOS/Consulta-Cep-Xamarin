using MvvmHelpers;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsultaCep.Data.Dto;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ConsultaCep.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        //<Button Text = "Buscar" Command="{Binding BuscarCep}" />
        //</StackLayout>
        private string _CEP;

        //<Entry Keyboard = "Numeric" Text="{Binding CEP}" />
        public const string CEPPropertyName = "CEP";
        public string CEP
        {
            get { return this._CEP; }
            set
            {
                // Se o novo valor informado pelo usuário não for nulo ou vazio...
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Limpamos o valor informado para que hajam somente números
                    this._CEP = Regex.Replace(value, "[^0-9]", string.Empty);

                    // Se o número de caracteres, após limpar o valor informado pelo usuário, for maior que 8, remove caracteres excedentes
                    this._CEP = this._CEP.Substring(0, this._CEP.Length > 8 ? 8 : this._CEP.Length);
                }
                else
                    // Se for nulo, simplesmente define o valor informado no atributo CEP
                    this._CEP = value;

                // Dispara os comandos na tela que informa ao mecanismo de Binding alteração nos valores da ViewModel
                OnPropertyChanged(CEPPropertyName);
                OnPropertyChanged(CEPMascaraPropertyName);
                OnPropertyChanged(IsCEPMascaraVisiblePropertyName);
            }
        }

        //<Label Text = "{Binding CEPMascara}" FontAttributes="Bold" IsVisible="{Binding IsCEPMascaraVisible}" />
        public const string CEPMascaraPropertyName = "CEPMascara";
        public string CEPMascara
        {
            get
            {
                /*
                * No GET, cria a máscara do CEP somente se o valor do atributo _CEP for válido
                */
                if (string.IsNullOrWhiteSpace(this._CEP))
                    return null;
                else if (this._CEP.Length != 8)
                    return null;
                else
                    return $"{this._CEP.Substring(0, 5)}-{this._CEP.Substring(5)}";
            }
        }

        public const string IsCEPMascaraVisiblePropertyName = "IsCEPMascaraVisible";
        public bool IsCEPMascaraVisible
        {
            get
            {
                /*
                * No GET, somente exibirá o Label da máscara de CEP se houver CEP com máscara para exibir
                */

                return !string.IsNullOrWhiteSpace(this.CEPMascara);
            }
        }

        private Command _BuscarCep;

        public Command BuscarCep
        {
            get
            {
                return this._BuscarCep ??
                    (this._BuscarCep =
                    new Command(async () => await BuscarCepExecute(),() => { return this.IsNotBusy; }));
            }
        }

        async Task BuscarCepExecute()
        {
            try
            {
                // Se a "ViewModel" estiver ocupada...
                if (this.IsBusy)

                    // Interrompe a chamada
                    return;

                // Define que a ViewModel está ocupada
                this.IsBusy = true;

                // Dispara informação que o comando BuscarCepCommand não pode ser executado, informando a interface que CanExecute mudou o status
                this.BuscarCep.ChangeCanExecute();

                // Se o CEP estiver vazio...
                if (string.IsNullOrWhiteSpace(this._CEP))
                    await App.Current.MainPage.DisplayAlert("Ah não!", "Você precisa informar um CEP, brow", "Ok");
                else
                {
                    
                    // Cria uma instância do objeto HttpClient que será usando para fazer a consulta ao WebService de busca de CEP
                    // COMO HttpClient implementa IDisposable, usamos o "using" para que "Dispose()" seja chamado automaticamente após encerrar o uso do objeto
                    using (var _client = new System.Net.Http.HttpClient())
                    {
                        //Realiza a chamada usando método GET e o retorno será carregado para a variável "_response"
                        // COMO HttpResponseMessage implementa IDisposable, assim como HttpClient, usamos o "using" novamente
                        using (var _response = await _client.GetAsync($"https://viacep.com.br/ws/{this._CEP}/json/"))
                        {
                            // Se a mensagem HTTP não possui um Status que representa sucesso
                            if (!_response.IsSuccessStatusCode)
                                await App.Current.MainPage.DisplayAlert("Ooops...", "Algo de errado não está certo", "Ok");

                            // Lê o conteúdo da mensagem HTTP para uma string
                            var _responseContent = await _response.Content.ReadAsStringAsync();

                            // Deserializa a string convertendo o texto em formato JSON para objeto RetornoCepDto
                            var objRetorno = JsonConvert.DeserializeObject<RetornoCepDto>(_responseContent);

                            

                            await App
                                .Current
                                .MainPage
                                .DisplayAlert(
                                    "Parabéns",
                                    _responseContent,
                                    "Ok");
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                // Libera a "ViewModel"
                this.IsBusy = false;

                // Dispara informação que o comando BuscarCepCommand PODE ser executado, informando a interface que CanExecute mudou o status
                this.BuscarCep.ChangeCanExecute();

                this.RefreshCommand.Execute(null);
            }
        }

        public ObservableCollection<RetornoCepDto> _Retornos { get; private set; } =new ObservableCollection<RetornoCepDto>();

        public MainViewModel()
        {

            this.RefreshCommand.Execute(null);
           
        }

        private Command _RefreshCommand;

        public const string RefreshCommandPropertyName = "RefreshCommand";

        public Command RefreshCommand
        {
            get
            {
                if (this._RefreshCommand == null)
                {
                   this._RefreshCommand = new Command(async () => { await RefreshCommandExecute(); }, ()=> this.IsNotBusy); 
                }
                return this._RefreshCommand;
            }
        }

        async Task RefreshCommandExecute()
        {
            this.RefreshCommand.ChangeCanExecute();

            await Task.Factory.StartNew(() =>
            {
                //
            });

        }
    }
}
