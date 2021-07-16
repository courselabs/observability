package fulfilment.api;

import java.io.Serializable;
import java.math.BigDecimal;

public class Product implements Serializable {

    private long id;


    private String name;


    private BigDecimal price;

    public Product() {}

    public Product(String name, BigDecimal price) {
        setName(name);
        setPrice(price);
    }

    public long getId() {
    	return id;
    }
    
    public void setId(long id) {
        this.id = id;
    }

    public String getName() {
    	return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }

    public BigDecimal getPrice() {
    	return price;
    }
    
    public void setPrice(BigDecimal price) {
        this.price = price;
    }
}
