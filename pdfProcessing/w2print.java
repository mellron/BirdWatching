

/**
 * 
 */
package com.usbank.w2print;

import org.apache.pdfbox.io.MemoryUsageSetting;
import org.apache.pdfbox.multipdf.Splitter;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripperByArea;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FilenameFilter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.List;
import java.util.Map;
import java.util.regex.Pattern;
import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.awt.Rectangle;

/**
 * @author dxradha Version 2 - 1/4/2022 - Memory usage fix added temp file only
 *         Create Error batch for exceptions
 *         dxradha version 3 - 1/11/2022 - Remove unicode and special chars in pdf filename 
 * 
 *
 */
public class W2PrintVer2 {

    /**
     * @param args
     */
    private int batchSize, batchNum, pdfCount, errorCount;
    private String masterPath, outputDir, batchDir, errBatchDir;
    //private static final Pattern namePattern = Pattern.compile("[^\\w]", Pattern.UNICODE_CHARACTER_CLASS);
    private static final Pattern namePattern = Pattern.compile("[^a-zA-Z0-9]", Pattern.UNICODE_CHARACTER_CLASS);
    private String mainfestFileName;
    private OutputStreamWriter mainfestWriter;
    private String directorySep, manifestDelimeter, mainfestW2Constant;

    public W2PrintVer2(int batchSize, String masterPath, String outputDir) {
        super();
        this.batchSize = batchSize;
        this.masterPath = masterPath;
        this.batchNum = 0;
        this.pdfCount = 0;
        this.errorCount = 0;
        this.batchDir = "";
        this.errBatchDir = "";
        this.mainfestFileName = "";
        this.mainfestWriter = null;
        if (System.getProperty("os.name").startsWith("Windows")) {
            directorySep = "\\";
        } else {
            directorySep = "/";
        }
        this.manifestDelimeter = "|";
        this.mainfestW2Constant = "W2";
        SimpleDateFormat formatter = new SimpleDateFormat("MMddyyyy_HHmmss");
        Date date = new Date();
        this.outputDir = outputDir + this.directorySep + formatter.format(date);
    }

    public String addressExtract(PDDocument pdfDoc) throws IOException {
        PDFTextStripperByArea stripper = new PDFTextStripperByArea();
        stripper.setSortByPosition(true);
        Rectangle rect = new Rectangle(14, 162, 280, 54);
        stripper.addRegion("EmployeeAddress", rect);
        stripper.extractRegions(pdfDoc.getPage(0));
        String addressLines = stripper.getTextForRegion("EmployeeAddress");
        return addressLines;
    }

    public void setupBatch() throws IOException {

        this.batchDir = this.outputDir + this.directorySep + "W2sPDF" + String.format("%02d", ++batchNum);
        File directory = new File(batchDir);
        if (!directory.exists()) {
            directory.mkdirs();
        }
        if (this.mainfestWriter != null) {
            this.mainfestWriter.flush();
            this.mainfestWriter.close();
        }

        mainfestFileName = this.batchDir + this.directorySep + "W2sPDF" + String.format("%02d", batchNum)
                + "_Manifest.txt";
        mainfestWriter = new OutputStreamWriter(new FileOutputStream(mainfestFileName), StandardCharsets.UTF_8);

    }

    public void setupErrorBatch() throws IOException {

        this.errBatchDir = this.outputDir + this.directorySep + "W2sPDFError";
        File directory = new File(errBatchDir);
        if (!directory.exists()) {
            directory.mkdirs();
        }
    }

    public Map<String, String> getAddressElements(String address) throws Exception {
        Map<String, String> addressElements = new HashMap<String, String>();
        String empName, empAddress1 = null, empCity = null, empState = null, empZip = null, empFileName;
        String[] cityStateZip, stateZip;
        String pattern = Pattern.quote(System.lineSeparator());
        String lines[] = address.split(pattern);
        // Process Name
        if (lines.length < 3) {
            System.out.println("Address Extract Error. Total Address lines less than excepected");
            throw new Exception("Address Extract Error. Total Address lines less than excepected");
        }
        // First line is Name. Replace all non alphanumeric chars with ""
        empFileName = namePattern.matcher(lines[0]).replaceAll("");
        empName = lines[0];

        // Address is the second line
        empAddress1 = lines[1];

        // StateZip is the last line
        cityStateZip = lines[lines.length - 1].split(","); // City, State Zip
        empCity = cityStateZip[0];// City
        stateZip = cityStateZip[1].trim().split(" "); // State Zip
        empState = stateZip[0];
        empZip = stateZip[1];

        addressElements.put("Name", empName);
        addressElements.put("FileName", empFileName);
        addressElements.put("Address1", empAddress1);
        addressElements.put("City", empCity);
        addressElements.put("State", empState);
        addressElements.put("Zip", empZip);

        return addressElements;

    }

    public void processPDF() throws IOException {
        String addressesInfo = null;
        Map<String, String> addressElements = null;
        PDDocument pd;
        String formattedCount, splitPdfName;
        StringBuffer manifestFileStr;
        int filePdfCount = 0, fileErrorCount = 0;
        File dir = new File(this.masterPath);
        File[] files = dir.listFiles(new FilenameFilter() {
            @Override
            public boolean accept(File dir, String name) {
                return name.endsWith(".pdf");
            }
        });
        for (int fcount = 0; fcount < files.length; fcount++) {
            System.out.println("Processing " + files[fcount]);

            File pdfName = files[fcount];

            // *****Dhanabal. Verion 2 change start
            // PDDocument document = PDDocument.load(pdfName);
            PDDocument document = PDDocument.load(pdfName, MemoryUsageSetting.setupTempFileOnly());
            // *****Dhanabal. Verion 2 change End

            System.out.println("\tNumber of Pages " + document.getNumberOfPages());

            // instantiating Splitter
            Splitter splitter = new Splitter();

            // split the pages of a PDF document
            List<PDDocument> Pages = splitter.split(document);

            // Creating an iterator
            Iterator<PDDocument> iterator = Pages.listIterator();

            setupErrorBatch();

            filePdfCount = 0;
            fileErrorCount = 0;

            // saving splits as pdf
            while (iterator.hasNext()) {

                if (pdfCount % batchSize == 0) {
                    this.setupBatch();
                }

                pd = iterator.next();
                filePdfCount++;
                // provide destination path to the PDF split
                try {
                    addressesInfo = addressExtract(pd);
                    addressElements = getAddressElements(addressesInfo);

                    // System.out.println("Name " +
                    // addressElements.get("Name"));
                    // System.out.println("Address " +
                    // addressElements.get("Address1"));
                    // System.out.println("City " +
                    // addressElements.get("City"));
                    // System.out.println("State " +
                    // addressElements.get("State"));
                    // System.out.println("Zip " + addressElements.get("Zip"));

                    formattedCount = String.format("%05d", ++pdfCount);
                    splitPdfName = addressElements.get("FileName") + "_" + formattedCount + ".pdf";
                    pd.save(this.batchDir + this.directorySep + splitPdfName);
                    // write manifestfile

                    manifestFileStr = new StringBuffer("");
                    manifestFileStr.append(splitPdfName);
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(addressElements.get("Name"));
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(addressElements.get("Address1"));
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(addressElements.get("City"));
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(addressElements.get("State"));
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(addressElements.get("Zip"));
                    manifestFileStr.append(this.manifestDelimeter);
                    manifestFileStr.append(this.mainfestW2Constant);
                    manifestFileStr.append(System.lineSeparator());

                    mainfestWriter.write(manifestFileStr.toString());
                    pd.close();
                    System.out.println("\t" + splitPdfName);
                } catch (Exception e) {
                    fileErrorCount++;
                    System.out.println("\tERROR - Processing PDF. Moving the following PDF to Error Batch");
                    System.out.println("\t\t" + addressesInfo);
                    formattedCount = String.format("%05d", ++errorCount);
                    splitPdfName = "ErrorPDF" + "_" + formattedCount + ".pdf";
                    pd.save(this.errBatchDir + this.directorySep + splitPdfName);
                    pd.close();
                }

            }
            System.out.println("\tSplit Successfull.");
            System.out.println("------------------------------------------");
            System.out.println("\tPDFs Processed in this file - " + filePdfCount);
            System.out.println("\tError Records in this file  - " + fileErrorCount);
            System.out.println("------------------------------------------\n");
            document.close();
        }
        if (this.mainfestWriter != null) {
            this.mainfestWriter.flush();
            this.mainfestWriter.close();
        }

        System.out.println("****************************************************");
        System.out.println("\tTotal PDF Records Split - " + pdfCount);
        System.out.println("\tTotal Error Records - " + errorCount);
        System.out.println("****************************************************");

    }

    public static void main(String[] args) throws IOException {

        // Check the arguments
        if (args.length != 3) {
            System.out.println("Usage W2PrintVer2 <batchSize> <inputPDFDir> <OutputFoler>");
            System.exit(1);
        }

        System.out.println("Starting....");
        long startTime = System.currentTimeMillis();
        W2PrintVer2 w2Print = new W2PrintVer2(Integer.parseInt(args[0]), args[1], args[2]);
        w2Print.processPDF();
        long endTime = System.currentTimeMillis();
        System.out.println("\tProcess completed sucessfully. It took " + (endTime - startTime) / 1000 + " seconds");

    }
}

