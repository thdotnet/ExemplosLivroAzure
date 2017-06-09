function validaEvento(nomeEvento, nomeParticipante) {
    var context = getContext();
    var collection = context.getCollection();

    var query = "SELECT E.Nome FROM root E " +
                "join p in E.Participantes " +
                "where E.Nome  = '" + nomeEvento + "' " +
                "AND p.Nome = '" + nomeParticipante + "' ";

    collection.queryDocuments(collection.getSelfLink(),
       query, {},
       function (err, resposta, responseOptions) {
           if (err) throw new Error("Error" + err.message);

           if (resposta.length == 0)
               throw "Inscrição não realizada";

           context.getResponse().setBody("OK");
       }
    );
}