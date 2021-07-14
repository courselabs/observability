package widgetario.products;

import java.io.Serializable;

public class Document implements Serializable {

    private long id;
    private String fileName;
    private long size;
    private int fulfilmentStatus;

    public Document() {}

    public Document(long id, String fileName, long size, int fulfilmentStatus) {
        setId(id);
        setFileName(fileName);
        setSize(size);
        setFulfilmentStatus(fulfilmentStatus);
    }

    public long getId() {
    	return id;
    }
    
    public void setId(long id) {
        this.id = id;
    }

    public String getFileName() {
    	return fileName;
    }
    
    public void setFileName(String fileName) {
        this.fileName = fileName;
    }

    public long getSize() {
    	return size;
    }
    
    public void setSize(long size) {
        this.size = size;
    } 

    public int getFulfilmentStatus() {
    	return fulfilmentStatus;
    }
    
    public void setFulfilmentStatus(int fulfilmentStatus) {
        this.fulfilmentStatus = fulfilmentStatus;
    }
}
