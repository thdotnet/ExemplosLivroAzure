function normalizarNomeEvento() {
    var item = getContext().getRequest().getBody();
    item.Nome = item.Nome.toUpperCase();
    getContext().getRequest().setBody(item);
}