package widgetario.products;

import io.micrometer.core.annotation.Timed;
import io.micrometer.core.instrument.MeterRegistry;

import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;

import java.math.BigDecimal;
import java.math.MathContext;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.client.RestTemplate;

@RestController
public class DocumentsController {
    private static final Logger log = LoggerFactory.getLogger(DocumentsController.class);
    
    @Autowired
    MeterRegistry registry;    

    @RequestMapping("/documents")
    @Timed()
    public List<Document> get() {
        log.debug("** GET /documents called");
        
        // TODO-NEXT - baggage not propagating - w3c proto?
        Span span = GlobalTracer.get().activeSpan();
        if (span != null){
            String userId = span.getBaggageItem("userId");
            log.debug("** Got userId from span: " + userId);
        }

        List<Document> documents = new ArrayList<>();
        documents.add(new Document(12345, "contract.pdf", 4598798, 1));
        documents.add(new Document(234435, "timetable.docx", 2342134, 1));
        documents.add(new Document(141242, "obsfun.pptx", 900194123, 2));
        return documents;
    }
}
