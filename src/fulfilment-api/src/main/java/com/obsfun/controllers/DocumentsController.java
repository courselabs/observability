package fulfilment.api;

import io.micrometer.core.annotation.Timed;
import io.micrometer.core.instrument.MeterRegistry;

import io.opentracing.Span;
import io.opentracing.SpanContext;

import java.math.BigDecimal;
import java.math.MathContext;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.client.RestTemplate;

@RestController
public class DocumentsController {
    private static final Logger log = LoggerFactory.getLogger(DocumentsController.class);
    private static final List<Document> documents = new ArrayList<>();
    private static final Random random = new Random();

    @Autowired
    MeterRegistry registry;
    
    @Autowired
    private io.opentracing.Tracer tracer; 
    
    @Value("${trace.custom.spans}")
    private boolean customSpans;  

    public DocumentsController() {
        if (documents.size() == 0) {
            documents.add(new Document(generateDocumentId(), "contract.pdf", generateDocumentSize(), 1, "0421"));
            documents.add(new Document(generateDocumentId(), "timetable.docx", generateDocumentSize(), 1, "0443"));
            documents.add(new Document(generateDocumentId(), "obsfun.pptx", generateDocumentSize(), 2, "0401"));
            log.info("** Populated document list, count: " + documents.size());
        }
    }

    @RequestMapping("/documents")
    @Timed()
    public List<Document> get() {      
        Span span = tracer.activeSpan();
        if (span != null){
            log.debug("** GET /documents called in trace id: " + span.context().toTraceId());
        }
        else{
            log.debug("** GET /documents called");
        }


        Span dbLoadSpan = null;
        if (customSpans) {
            dbLoadSpan = tracer.buildSpan("database-load").start();
            dbLoadSpan.setTag("db.type", "sql");
            dbLoadSpan.setTag("db.instance", "documents");
            dbLoadSpan.setTag("db.statement", "SELECT * FROM documents");
            dbLoadSpan.setTag("span.kind", "internal");
        }

        try {
            log.info("** Returning documents, count: " + documents.size());
            return documents;
        }
        finally {
            if (dbLoadSpan != null) {
                dbLoadSpan.finish();
            }
        }
    }

    @PostMapping("/document")
    @Timed
    public Document submitDocument(@RequestBody Document document) {     
        Span span = tracer.activeSpan();
        if (span != null){
            log.debug("** POST /document called in trace id: " + span.context().toTraceId());
        }
        else{
            log.debug("** POST /document called");
        }

        document.setFulfilmentStatus(1);
        document.setId(generateDocumentId());
        document.setSize(generateDocumentSize());

        documents.add(document);
        log.info("** Added document ID: " + document.getId() + ", to list. Count: " + documents.size());
        return document;
    }

    private static int generateDocumentId() {
        return 20000000+random.nextInt(20000000);
    }

    private static int generateDocumentSize() {
        return (10000+random.nextInt(500000)) * random.nextInt(3) + random.nextInt(5000);
    }
}
